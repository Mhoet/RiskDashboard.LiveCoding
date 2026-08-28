namespace RiskDashboard.LiveCoding.Models;

public class RiskAssessment
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RiskId { get; set; }
    public decimal Score { get; set; }
    public DateTime AssessmentDate { get; set; }
}
