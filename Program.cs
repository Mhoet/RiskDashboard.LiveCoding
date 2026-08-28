using Microsoft.EntityFrameworkCore;
using RiskDashboard.LiveCoding.Data;
using RiskDashboard.LiveCoding.Models;
using RiskDashboard.LiveCoding.Services;
using RiskDashboard.LiveCoding.Initialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("RiskDashboardLiveCodingDb");
});

builder.Services.AddScoped<RiskDashboardService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// I moved this to a separate class
DBSeeder.Seed(app);

app.Run();