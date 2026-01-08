using Fundo.Application;
using Fundo.Infrastructure;
using Fundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var policyName = "AllowFrontend";

// Configure Infrastructure services (EF Core, Repositories)
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

// Add CORS for frontend integration
var allowedOrigins = builder.Configuration.GetValue<string>("CorsOrigins") 
    ?? "http://localhost:4200";
    
builder.Services.AddCors(options =>
{
    options.AddPolicy(policyName, policy =>
    {
        policy.WithOrigins(allowedOrigins.Split(';'))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Apply migrations on startup (for Docker)
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

app.UseRouting();
app.UseCors(policyName);
app.MapControllers();

await app.RunAsync();

public partial class Program { }