﻿using Microsoft.EntityFrameworkCore;
using EfIdentityDbContext = Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext;

namespace Sample.Identity.Infrastructure.Data;

public sealed class IdentityDbContext : EfIdentityDbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }
}
