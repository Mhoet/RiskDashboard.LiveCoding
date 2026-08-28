namespace RiskDashboard.LiveCoding.Models;

public class Risk
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BusinessUnitId { get; set; }
    public string Title { get; set; }
    public DateTime CreatedDate { get; set; }

    public ICollection<RiskControl> Controls { get; set; }
    public ICollection<RiskAssessment> Assessments { get; set; }
}
