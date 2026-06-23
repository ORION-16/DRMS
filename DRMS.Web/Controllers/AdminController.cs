using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DRMS.Application.Services.Interfaces;

namespace DRMS.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    private readonly IDeploymentRequestService _requestService;
    private readonly IMasterDataService _masterDataService;

    public AdminController(IDeploymentRequestService requestService, IMasterDataService masterDataService)
    {
        _requestService = requestService;
        _masterDataService = masterDataService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? statusId, int? projectId)
    {
        ViewBag.Statuses = new SelectList(await _masterDataService.GetStatusesAsync(), "StatusId", "StatusName", statusId);
        ViewBag.Projects = new SelectList(await _masterDataService.GetProjectsAsync(), "ProjectId", "ProjectName", projectId);
        
        var requests = await _requestService.GetAllAsync(statusId, projectId);
        return View(requests);
    }

    [HttpGet]
    public async Task<IActionResult> History(int id)
    {
        var request = await _requestService.GetByIdAsync(id);
        if (request == null)
            return NotFound();

        return View(request);
    }
}
