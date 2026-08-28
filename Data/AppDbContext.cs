using Microsoft.EntityFrameworkCore;
using RiskDashboard.LiveCoding.Models;

namespace RiskDashboard.LiveCoding.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Risk> Risks { get; set; }
    public DbSet<RiskControl> Controls { get; set; }
    public DbSet<RiskAssessment> RiskAssessments { get; set; }
}
