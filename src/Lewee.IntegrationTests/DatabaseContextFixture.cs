using DotNet.Testcontainers.Builders;
using Lewee.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Testcontainers.MsSql;
using Xunit;

namespace Lewee.IntegrationTests;

/// <summary>
/// Database Context Fixture
/// </summary>
/// <typeparam name="TDbContext">Database context type</typeparam>
/// <typeparam name="TDbSeeder">Database seeder type</typeparam>
public abstract class DatabaseContextFixture<TDbContext, TDbSeeder> : IAsyncLifetime
    where TDbContext : DbContext
    where TDbSeeder : IDatabaseSeeder<TDbContext>
{
    private readonly MsSqlContainer msSqlContainer;
    private bool isDbInitialized = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseContextFixture{TDbContext, TDbSeeder}"/> class
    /// </summary>
    protected DatabaseContextFixture()
    {
        this.msSqlContainer = new MsSqlBuilder()
            .WithImage($"mcr.microsoft.com/mssql/server:{this.MsSqlImageVersion}")
            .WithPortBinding(1433, 1433) // Bind the standard MS SQL port
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433)) // Wait until the port is available
            .Build();
    }

    /// <summary>
    /// Gets the MS SQL image version e.g. "2022-latest"
    /// </summary>
    protected abstract string MsSqlImageVersion { get; }

    /// <summary>
    /// Gets the database connection string
    /// </summary>
    protected string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the database reset options
    /// </summary>
    protected abstract RespawnerOptions ResetOptions { get; }

    /// <summary>
    /// Gets the environment name
    /// </summary>
    protected abstract string EnvironmentName { get; }

    /// <summary>
    /// Gets the connection string name used in appsettings.json
    /// </summary>
    protected abstract string ConnectionStringName { get; }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await this.msSqlContainer.StartAsync();
        this.ConnectionString = this.msSqlContainer.GetConnectionString();
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await this.msSqlContainer.StopAsync();
        await this.msSqlContainer.DisposeAsync();
    }

    /// <summary>
    /// Resets the database
    /// </summary>
    /// <returns>An asynchronous task</returns>
    public async Task ResetDatabase()
    {
        var dbContext = this.CreateDbContext();
        var seeder = this.CreateDbSeeder(dbContext);

        if (!this.isDbInitialized)
        {
            await dbContext.Database.MigrateAsync();
            this.isDbInitialized = true;
        }

        try
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                await connection.OpenAsync();
            }
        }
        catch (SqlException)
        {
            // Database doesn't exist yet, probably because migrations haven't been run to create it
            return;
        }

        var respawner = await Respawner.CreateAsync(this.ConnectionString, this.ResetOptions);
        await respawner.ResetAsync(this.ConnectionString);

        await seeder.Run();
    }

    /// <summary>
    /// Creates a database context
    /// </summary>
    /// <returns>Database context</returns>
    protected abstract TDbContext CreateDbContext();

    /// <summary>
    /// Create a database seeder
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>Database seeder</returns>
    protected abstract TDbSeeder CreateDbSeeder(TDbContext? context = null);
}
