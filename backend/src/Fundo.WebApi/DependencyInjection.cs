using Fundo.WebApi.Constants;
using System.Reflection;
using System.Text.Json;

namespace Fundo.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add CORS for frontend integration
        var allowedOrigins = configuration.GetValue<string>("CorsOrigins")
                             ?? "http://localhost:4200";

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyConstants.AllowFrontend, policy =>
            {
                policy.WithOrigins(allowedOrigins.Split(';'))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Enforce camelCase for JSON (industry standard)
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                // Make property matching case-sensitive for stricter API contract
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
            });

        // Configure Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Version = "v1",
                Title = "Fundo Loan Management API",
                Description = "RESTful API for managing loans with Clean Architecture",
                Contact = new()
                {
                    Name = "Fundo Team",
                    Url = new Uri("https://github.com/cristofima/take-home-test")
                }
            });

            // Enable XML comments for better documentation
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}