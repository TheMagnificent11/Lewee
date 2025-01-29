using System.Data;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Lewee.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lewee.IntegrationTests;

/// <summary>
/// Web API Integration Tests
/// </summary>
/// <typeparam name="TEntryPoint">ASP.Net app entry point class</typeparam>
/// <typeparam name="TFactory">Web application factory type</typeparam>
/// <typeparam name="TDbContextFixture">Database context fixture type</typeparam>
/// <typeparam name="TDbContext">Database context type</typeparam>
/// <typeparam name="TDbSeeder">Database seeder type</typeparam>
public abstract class WebApiIntegrationTests<TEntryPoint, TFactory, TDbContextFixture, TDbContext, TDbSeeder>
    : IClassFixture<TFactory>, IAsyncLifetime
    where TEntryPoint : class
    where TFactory : WebApplicationFactory<TEntryPoint>
    where TDbContextFixture : DatabaseContextFixture<TDbContext, TDbSeeder>, new()
    where TDbContext : DbContext
    where TDbSeeder : IDatabaseSeeder<TDbContext>
{
    private readonly TDbContextFixture dbContextFixture;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="WebApiIntegrationTests{TEntryPoint, TFactory, TDbContextFixture, TDbContext, TDbSeeder}"/> class
    /// </summary>
    /// <param name="factory">Web application factory</param>
    /// <param name="dbContextFixture">Database context fixture</param>
    protected WebApiIntegrationTests(TFactory factory, TDbContextFixture dbContextFixture)
    {
        this.Factory = factory;
        this.dbContextFixture = dbContextFixture;
    }

    /// <summary>
    /// Gets the Web Application Factory
    /// </summary>
    protected TFactory Factory { get; }

    private IServiceScopeFactory ScopeFactory
    {
        get
        {
            var scopeFactory = this.Factory.Services.GetService<IServiceScopeFactory>()
                ?? throw new InvalidOperationException("Could not get scope factory");

            return scopeFactory;
        }
    }

    /// <inheritdoc />
    public virtual async Task InitializeAsync()
    {
        await this.dbContextFixture.InitializeAsync();
        await this.dbContextFixture.ResetDatabase();
    }

    /// <inheritdoc />
    public virtual async Task DisposeAsync()
    {
        await this.dbContextFixture.DisposeAsync();
    }

    /// <summary>
    /// Creates a HTTP request message for the specified <paramref name="httpMethod"/> and <paramref name="apiPath"/>
    /// with <paramref name="content"/> has the request body
    /// </summary>
    /// <param name="httpMethod">HTTP method</param>
    /// <param name="apiPath">API path</param>
    /// <param name="content">Request body content</param>
    /// <returns>A HTTP request message</returns>
    protected static HttpRequestMessage CreateHttpRequestMessage(HttpMethod httpMethod, string apiPath, object content)
    {
        var request = new HttpRequestMessage(httpMethod, apiPath)
        {
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };

        return request;
    }

    /// <summary>
    /// Executes a HTTP GET request
    /// </summary>
    /// <typeparam name="T">Expected HTTP GET response type</typeparam>
    /// <param name="apiPath">API path</param>
    /// <returns>The deserialized body from the response to the HTTP GET request</returns>
    protected async Task<T?> HttpGet<T>(string apiPath)
    {
        using (var response = await this.HttpRequest(HttpMethod.Get, apiPath))
        {
            return await this.DeserializeResponse<T>(response);
        }
    }

    /// <summary>
    /// Executes a HTTP request
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="apiPath">API path</param>
    /// <returns>The HTTP response</returns>
    protected async Task<HttpResponseMessage> HttpRequest(HttpMethod method, string apiPath)
    {
        // TODO (https://github.com/TheMagnificent11/lewee/issues/14): add request body parameter
        using (var request = new HttpRequestMessage(method, apiPath))
        using (var httpClient = this.Factory.CreateClient())
        {
            return await httpClient.SendAsync(request);
        }
    }

    /// <summary>
    /// Calls the health check endpoint and ensures a success status code
    /// </summary>
    /// <param name="healthPath">Health path</param>
    /// <returns>An asynchronous task</returns>
    protected async Task HealthCheck(string healthPath)
    {
        using (var response = await this.HttpRequest(HttpMethod.Get, healthPath))
        {
            response.EnsureSuccessStatusCode();
        }
    }

    /// <summary>
    /// Deserializes a HTTP response
    /// </summary>
    /// <typeparam name="T">Response body type</typeparam>
    /// <param name="response">Response to deserialize</param>
    /// <param name="isSuccess">Whether the response was expected successful</param>
    /// <returns>The deserialized data</returns>
    protected virtual async Task<T?> DeserializeResponse<T>(HttpResponseMessage response, bool isSuccess = true)
    {
        if (isSuccess)
        {
            response.EnsureSuccessStatusCode();
        }

        return await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Waits three seconds for domain events to be dispatched
    /// </summary>
    /// <returns>Asynchronous task</returns>
    protected async Task WaitForDomainEventsToBeDispatched()
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
    }

    /// <summary>
    /// Gets a service of type <typeparamref name="T"/> from the dependency injection container
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>The service if it exists, otherwise null</returns>
    protected T? GetService<T>()
        where T : class
    {
        return this.ScopeFactory
            .CreateScope()
            .ServiceProvider
            .GetService<T>();
    }

    /// <summary>
    /// Reads data from the targeted <typeparamref name="TDbContext"/>
    /// </summary>
    /// <typeparam name="TData">DB context set type</typeparam>
    /// <param name="predicate">Predicate for query</param>
    /// <returns>A list of matched data objects</returns>
    protected async Task<List<TData>> GetData<TData>(Expression<Func<TData, bool>> predicate)
        where TData : class
    {
        using (var scope = this.ScopeFactory.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;

            var dbContext = scopedServices.GetRequiredService<TDbContext>();
            if (dbContext == null)
            {
                return new List<TData>();
            }

            return await dbContext
                .Set<TData>()
                .Where(predicate)
                .ToListAsync();
        }
    }
}
