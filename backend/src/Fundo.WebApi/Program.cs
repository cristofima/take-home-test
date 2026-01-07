using Fundo.Application;
using Fundo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Infrastructure services (EF Core, Repositories)
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

// Add CORS for frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseCors("AllowFrontend");
app.MapControllers();

await app.RunAsync();

public partial class Program { }