﻿using System.Text.Json;
using Lewee.Application;
using Lewee.Infrastructure.AspNet.Auth;
using Lewee.Infrastructure.AspNet.Logging;
using Lewee.Infrastructure.AspNet.Settings;
using Lewee.Infrastructure.AspNet.SignalR;
using Lewee.Infrastructure.AspNet.WebApi;
using Lewee.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Sample.Restaurant.Application;
using Sample.Restaurant.Infrastructure.Data;

namespace Sample.Restaurant.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

        var connectionString = builder.Configuration.GetConnectionString("Sample.Restaurant");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ApplicationException("Could not find database connection string");
        }

        var appSettings = builder.Configuration.GetSettings<ApplicationSettings>(nameof(ApplicationSettings));
        var seqSettings = builder.Configuration.GetSettings<SeqSettings>(nameof(SeqSettings));
        var migrateDatabases = builder.Configuration.GetValue<bool>("MigrateDatabases");
        /* var serviceBusSettings = builder.Configuration.GetSettings<ServiceBusSettings>(nameof(ServiceBusSettings)); */

        builder.Host.ConfigureLogging(appSettings, seqSettings);

        builder.Services.AddMapper();

        builder.Services
            .ConfigureDatabaseWithSeeder<RestaurantDbContext, RestaurantDbSeeder>(connectionString)
            .ConfigureAuthenticatedUserService()
            .ConfigureRestaurantData() // TODO: ideally this would not be needed if IRepository would be registered globally
#if DEBUG
            .AddDatabaseDeveloperPageExceptionFilter()
#endif
            /* .ConfigureServiceBusPublisher(serviceBusSettings) */
            .AddRestaurantApplication();

        builder.Services
            .AddControllersWithViews(options =>
            {
                options.Conventions.Add(new KebabCaseControllerModelConvention());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .ConfigureSignalR()
            .AddHealthChecks()
            .AddDbContextCheck<RestaurantDbContext>();

        builder.Services.AddRazorPages();

        var app = builder.Build();

        app.UseResponseCompression();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHealthChecks("/health")
            .UseHttpsRedirection()
            .UseBlazorFrameworkFiles()
            .UseStaticFiles()
            .UseRouting();

        app.MapRazorPages();
        app.MapControllers();
        app.MapHub<ClientEventHub>("/events");
        app.MapFallbackToFile("index.html");

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (migrateDatabases)
        {
            await app.MigrationDatabase<RestaurantDbContext>(seedData: true);
        }

        app.Run();
    }
}
