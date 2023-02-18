﻿using Lewee.Domain;
using Microsoft.EntityFrameworkCore;

namespace Lewee.Application.Data;

/// <summary>
/// Application Database Context Interface
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets the domain event references database set
    /// </summary>
    DbSet<DomainEventReference>? DomainEventReferences { get; }

    /// <summary>
    /// Gets the query projection references database set
    /// </summary>
    DbSet<QueryProjectionReference>? QueryProjectionReferences { get; }
}
