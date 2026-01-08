using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Fundo.Infrastructure.Data;
using Testcontainers.MsSql;
using Xunit;

namespace Fundo.Services.Tests.Integration.Fundo.WebApi;

/// <summary>
/// Custom WebApplicationFactory that configures TestContainers SQL Server for integration testing.
/// Uses a real SQL Server instance in Docker to provide production-parity testing.
/// </summary>
/// <remarks>
/// See: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#customize-webapplicationfactory
/// </remarks>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Strong_Password_123!")
        .Build();

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to "Test" to disable automatic migrations in Program.cs
        builder.UseEnvironment("Test");
        
        builder.ConfigureTestServices(services =>
        {
            // Remove SQL Server DbContext registration from production
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<LoanDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register DbContext with TestContainers SQL Server connection string
            services.AddDbContext<LoanDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString());
            });
        });

        // Initialize database schema after test services are configured
        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
