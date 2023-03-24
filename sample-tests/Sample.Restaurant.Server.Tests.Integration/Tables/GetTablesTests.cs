﻿using FluentAssertions;
using Sample.Restaurant.Application;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Sample.Restaurant.Server.Tests.Integration.Tables;

public sealed class GetTablesTests : TabeTestsBase
{
    public GetTablesTests(RestaurantWebApplicationFactory factory)
        : base(factory)
    {
    }

    private TableDto[] Tables { get; set; }

    [BddfyFact]
    public void GetTables_ShouldReturnTheCorrectTables()
    {
        this.Given(x => this.AnEmptyRestaurant())
            .When(x => this.GetTablesRequestIsExecuted())
            .Then(x => x.TheCorrectTablesAreReturned());
    }

    private async Task GetTablesRequestIsExecuted()
    {
        this.Tables = await this.HttpGet<TableDto[]>("/tables");
    }

    private void TheCorrectTablesAreReturned()
    {
        this.Tables.Should().NotBeNull();
        this.Tables.Should().HaveCount(10);

        for (var i = 1; i <= 10; i++)
        {
            var table = this.Tables[i];
            table.Should().NotBeNull();
            table.Id.Should().NotBeEmpty();
            table.TableNumber.Should().Be(i);
            table.Status.Should().Be(TableDto.Statuses.Available);
        }
    }
}
