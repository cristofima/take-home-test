using Fundo.Application;
using Fundo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var policyName = "AllowFrontend";

// Configure Infrastructure services (EF Core, Repositories)
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

// Add CORS for frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy(policyName, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
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
}

app.UseRouting();
app.UseCors(policyName);
app.MapControllers();

await app.RunAsync();

public partial class Program { }