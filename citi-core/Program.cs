using citi_core.Data;
using citi_core.Interfaces;
using citi_core.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetValue<string>("DatabaseSettings:ConnectionString")));

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register repositories
builder.Services.AddScoped<IUserRepository, DbUserRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();  

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services, app.Environment);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
