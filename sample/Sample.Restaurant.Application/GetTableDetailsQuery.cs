﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lewee.Application.Mediation;
using Lewee.Application.Mediation.Responses;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sample.Restaurant.Domain;
using Serilog;

namespace Sample.Restaurant.Application;

public sealed class GetTableDetailsQuery : IQuery<QueryResult<TableDetailsDto>>
{
    public GetTableDetailsQuery(Guid correlationId, int tableNumber)
    {
        this.CorrelationId = correlationId;
        this.TableNumber = tableNumber;
    }

    public Guid CorrelationId { get; }
    public int TableNumber { get; }

    internal class GetTableDetailsQueryHandler : IRequestHandler<GetTableDetailsQuery, QueryResult<TableDetailsDto>>
    {
        private readonly IRestaurantDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public GetTableDetailsQueryHandler(IRestaurantDbContext dbContext, IMapper mapper, ILogger logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger.ForContext<GetTableDetailsQueryHandler>();
        }

        public async Task<QueryResult<TableDetailsDto>> Handle(GetTableDetailsQuery request, CancellationToken cancellationToken)
        {
            var table = await this.dbContext
                .AggregateRoot<Table>()
                .Include(x => x.Orders)
                .ThenInclude(x => x.Items)
                .FirstOrDefaultAsync(x => x.TableNumber == request.TableNumber, cancellationToken);

            if (table == null)
            {
                this.logger.Warning("Table not found");
                return QueryResult<TableDetailsDto>.Fail(ResultStatus.NotFound, "Table not found");
            }

            var tableDto = this.mapper.Map<TableDto>(table);

            var menuItems = await this.dbContext
                .AggregateRoot<MenuItem>()
                .OrderBy(x => x.ItemTypeId)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var menuItemDtos = this.mapper.Map<List<MenuItemDto>>(menuItems);

            var tableDetailsDto = TableDetailsDto.Create(tableDto, menuItemDtos);

            if (!table.IsInUse || table.CurrentOrder == null)
            {
                return QueryResult<TableDetailsDto>.Success(tableDetailsDto);
            }

            foreach (var item in table.CurrentOrder.Items)
            {
                var itemDto = tableDetailsDto.Items.FirstOrDefault(x => x.MenuItem.Id == item.MenuItemId);
                if (itemDto == null)
                {
                    continue;
                }

                itemDto.Quantity = item.Quantity;
            }

            return QueryResult<TableDetailsDto>.Success(tableDetailsDto);
        }
    }
}