namespace RiskDashboard.LiveCoding.Models;

public class RiskDashboardDto
{
    public Guid RiskId { get; set; }
    public string RiskTitle { get; set; }
    public Guid BusinessUnitId { get; set; }
    public int ControlCount { get; set; }
    public DateTime? LatestAssessmentDate { get; set; }
    public decimal AverageAssessmentScore { get; set; }
    public string RiskRating { get; set; }
}
