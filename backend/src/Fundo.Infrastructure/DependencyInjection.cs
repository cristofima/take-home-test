using Fundo.Application.Interfaces;
using Fundo.Infrastructure.Data;
using Fundo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fundo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Configure Entity Framework Core with SQL Server
        services.AddDbContext<LoanDbContext>(options =>
        {
            // Use Windows Authentication for local SQL Server
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        // Register repositories
        services.AddScoped<ILoanRepository, LoanRepository>();

        return services;
    }
}