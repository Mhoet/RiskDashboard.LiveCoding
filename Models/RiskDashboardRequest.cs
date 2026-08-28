namespace RiskDashboard.LiveCoding.Models;

public class RiskDashboardRequest
{
    public Guid TenantId { get; set; }
    public Guid? BusinessUnitId { get; set; }
    public string? SearchText { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
