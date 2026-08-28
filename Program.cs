using Microsoft.EntityFrameworkCore;
using RiskDashboard.LiveCoding.Data;
using RiskDashboard.LiveCoding.Models;
using RiskDashboard.LiveCoding.Services;

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

SeedData(app);

app.Run();

static void SeedData(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (db.Risks.Any())
    {
        return;
    }

    var tenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var tenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");
    var businessUnitA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    var businessUnitB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    var risks = new List<Risk>();
    var controls = new List<RiskControl>();
    var assessments = new List<RiskAssessment>();

    for (var i = 1; i <= 200; i++)
    {
        var tenantId = i <= 150 ? tenantA : tenantB;
        var businessUnitId = i % 2 == 0 ? businessUnitA : businessUnitB;
        var riskId = Guid.NewGuid();

        risks.Add(new Risk
        {
            Id = riskId,
            TenantId = tenantId,
            BusinessUnitId = businessUnitId,
            Title = $"Risk {i}",
            CreatedDate = DateTime.UtcNow.AddDays(-i),
            Controls = new List<RiskControl>(),
            Assessments = new List<RiskAssessment>()
        });

        for (var c = 1; c <= 3; c++)
        {
            controls.Add(new RiskControl
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RiskId = riskId,
                Name = $"Control {c} for Risk {i}",
                Status = c % 2 == 0 ? "Active" : "Pending"
            });
        }

        if (i % 10 != 0)
        {
            for (var a = 1; a <= 4; a++)
            {
                assessments.Add(new RiskAssessment
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RiskId = riskId,
                    Score = (i + a * 7) % 100,
                    AssessmentDate = DateTime.UtcNow.AddDays(-(i + a))
                });
            }
        }
    }

    db.Risks.AddRange(risks);
    db.Controls.AddRange(controls);
    db.RiskAssessments.AddRange(assessments);
    db.SaveChanges();
}
