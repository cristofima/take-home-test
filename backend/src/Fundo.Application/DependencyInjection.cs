using Fundo.Application.Interfaces;
using Fundo.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fundo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<ILoanService, LoanService>();

        return services;
    }
}