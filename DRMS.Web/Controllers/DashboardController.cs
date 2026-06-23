using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DRMS.Application.Services.Interfaces;
using System.Security.Claims;

namespace DRMS.Web.Controllers;

[Authorize(Policy = "DeveloperOnly")]
public class DashboardController : Controller
{
    private readonly IDeploymentRequestService _requestService;

    public DashboardController(IDeploymentRequestService requestService)
    {
        _requestService = requestService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roleId = 1; // Developer role ID

        var summary = await _requestService.GetDashboardSummaryAsync(userId, roleId);
        var recentRequests = await _requestService.GetByUserAsync(userId);

        ViewBag.Summary = summary;
        return View(recentRequests);
    }
}
