using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RiskDashboard.LiveCoding.Data;
using RiskDashboard.LiveCoding.Models;

namespace RiskDashboard.LiveCoding.Services;

public class RiskDashboardService
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RiskDashboardService> _logger;

    public RiskDashboardService(
        AppDbContext db,
        IDistributedCache cache,
        ILogger<RiskDashboardService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<RiskDashboardDto>> GetDashboardSummary(
        RiskDashboardRequest request)
    {
        var cacheKey = "risk-dashboard-summary";

        var cached = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrWhiteSpace(cached))
        {
            return JsonSerializer.Deserialize<List<RiskDashboardDto>>(cached);
        }

        var risks = _db.Risks
            .Include(x => x.Controls)
            .Include(x => x.Assessments)
            .Where(x => x.TenantId == request.TenantId)
            .ToList();

        if (request.BusinessUnitId.HasValue)
        {
            risks = risks
                .Where(x => x.BusinessUnitId == request.BusinessUnitId.Value)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            risks = risks
                .Where(x => x.Title.ToLower().Contains(request.SearchText.ToLower()))
                .ToList();
        }

        if (request.FromDate.HasValue)
        {
            risks = risks
                .Where(x => x.CreatedDate >= request.FromDate.Value)
                .ToList();
        }

        if (request.ToDate.HasValue)
        {
            risks = risks
                .Where(x => x.CreatedDate <= request.ToDate.Value)
                .ToList();
        }

        var result = new List<RiskDashboardDto>();

        foreach (var risk in risks)
        {
            var controlCount = _db.Controls
                .Where(x => x.RiskId == risk.Id)
                .Count();

            var latestAssessment = _db.RiskAssessments
                .Where(x => x.RiskId == risk.Id)
                .OrderByDescending(x => x.AssessmentDate)
                .FirstOrDefault();

            var averageScore = _db.RiskAssessments
                .Where(x => x.RiskId == risk.Id)
                .Average(x => x.Score);

            result.Add(new RiskDashboardDto
            {
                RiskId = risk.Id,
                RiskTitle = risk.Title,
                BusinessUnitId = risk.BusinessUnitId,
                ControlCount = controlCount,
                LatestAssessmentDate = latestAssessment.AssessmentDate,
                AverageAssessmentScore = averageScore,
                RiskRating = averageScore >= 75
                    ? "High"
                    : averageScore >= 40
                        ? "Medium"
                        : "Low"
            });
        }

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

        return result;
    }
}
