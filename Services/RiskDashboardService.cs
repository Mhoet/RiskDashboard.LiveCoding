using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RiskDashboard.LiveCoding.Data;
using RiskDashboard.LiveCoding.Models;

namespace RiskDashboard.LiveCoding.Services;

public class RiskDashboardService
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RiskDashboardService> _logger;

    public RiskDashboardService(AppDbContext db, IDistributedCache cache, ILogger<RiskDashboardService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<RiskDashboardDto>> GetDashboardSummary(RiskDashboardRequest request)
    {
        try
        {
            //Using dynamic cache key based on request parameters passed
            var cacheKey = $"risk-dashboard-summary:{request.TenantId}:{request.BusinessUnitId}:{request.FromDate?.Ticks}:{request.ToDate?.Ticks}:{request.SearchText?.Trim().ToLower()}";

        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            return JsonSerializer.Deserialize<List<RiskDashboardDto>>(cached) ?? new List<RiskDashboardDto>();
        }
            var query = _db.Risks
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId);

            if (request.BusinessUnitId.HasValue)
            {
                query = query.Where(x => x.BusinessUnitId == request.BusinessUnitId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText.Trim().ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(search));
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate <= request.ToDate.Value);
            }

            //Using DTO here instead of
            var result = await query
                .Select(risk => new RiskDashboardDto
                {
                    RiskId = risk.Id,
                    RiskTitle = risk.Title,
                    BusinessUnitId = risk.BusinessUnitId,
                    ControlCount = risk.Controls.Count,
                    LatestAssessmentDate = risk.Assessments
                        .OrderByDescending(a => a.AssessmentDate)
                        .Select(a => (DateTime?)a.AssessmentDate)
                        .FirstOrDefault(),
                    AverageAssessmentScore = (decimal)(risk.Assessments.Any()
                        ? risk.Assessments.Average(a => (double)a.Score)
                        : 0),
                    RiskRating = "Pending"
                })
                .ToListAsync();

            //prcess rating logic in memory 
            foreach (var dto in result)
            {
                dto.RiskRating = dto.AverageAssessmentScore >= 75
                    ? "High"
                    : dto.AverageAssessmentScore >= 40
                        ? "Medium"
                        : "Low";
            }

            //Cache the final result
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), cacheOptions);


            return result;

        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex}");
            return new List<RiskDashboardDto>();
        }
    }
}