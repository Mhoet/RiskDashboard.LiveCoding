using Microsoft.AspNetCore.Mvc;
using RiskDashboard.LiveCoding.Models;
using RiskDashboard.LiveCoding.Services;

namespace RiskDashboard.LiveCoding.Controllers;

[ApiController]
[Route("api/risk-dashboard")]
public class RiskDashboardController : ControllerBase
{
    private readonly RiskDashboardService _service;

    public RiskDashboardController(RiskDashboardService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    public IActionResult GetSummary([FromQuery] RiskDashboardRequest request)
    {
        var result = _service.GetDashboardSummary(request).Result;

        return Ok(result);
    }
}
