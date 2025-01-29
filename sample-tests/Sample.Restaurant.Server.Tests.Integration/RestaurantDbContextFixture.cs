using Lewee.Domain;
using Lewee.IntegrationTests;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Respawn.Graph;
using Sample.Restaurant.Infrastructure.Data;

namespace Sample.Restaurant.Server.Tests.Integration;

public sealed class RestaurantDbContextFixture : DatabaseContextFixture<RestaurantDbContext, RestaurantDbSeeder>
{
    private const string RestaurantDbSchema = "res";

    protected override RespawnerOptions ResetOptions => new()
    {
        DbAdapter = DbAdapter.SqlServer,
        SchemasToExclude = new[] { "dbo" },
        TablesToIgnore = new[]
        {
            new Table(RestaurantDbSchema, nameof(RestaurantDbContext.MenuItems)),
            new Table(RestaurantDbSchema, "OrderStatuses"),
            new Table(RestaurantDbSchema, "MenuItemTypes")
        }
    };

    protected override string EnvironmentName => "Testing";

    protected override string ConnectionStringName => "Sample.Restaurant";

    protected override string MsSqlImageVersion => "2022-latest";

    protected override RestaurantDbContext CreateDbContext()
    {
        var dbContextOptions = new DbContextOptionsBuilder<RestaurantDbContext>()
            .UseSqlServer(this.ConnectionString)
            .Options;

        var dbContext = new RestaurantDbContext(dbContextOptions, new TestAuthenticatedUserService());
        return dbContext;
    }

    protected override RestaurantDbSeeder CreateDbSeeder(RestaurantDbContext dbContext)
    {
        dbContext ??= this.CreateDbContext();

        return new RestaurantDbSeeder(dbContext);
    }

    private class TestAuthenticatedUserService : IAuthenticatedUserService
    {
        public string UserId => "Integration Tests";
    }
}
