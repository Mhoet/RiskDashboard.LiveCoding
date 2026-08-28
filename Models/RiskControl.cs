namespace RiskDashboard.LiveCoding.Models;

public class RiskControl
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RiskId { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
}
