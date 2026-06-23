using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DRMS.Application.Services.Interfaces;
using DRMS.Application.DTOs;
using System.Security.Claims;

namespace DRMS.Web.Controllers;

[Authorize(Policy = "DeveloperOnly")]
public class DeploymentController : Controller
{
    private readonly IDeploymentRequestService _requestService;
    private readonly IMasterDataService _masterDataService;

    public DeploymentController(IDeploymentRequestService requestService, IMasterDataService masterDataService)
    {
        _requestService = requestService;
        _masterDataService = masterDataService;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();
        return View(new CreateDeploymentRequestDto { RequestedDeploymentDate = DateTime.Now.AddDays(1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDeploymentRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _requestService.CreateAsync(model, userId);
            
            TempData["SuccessMessage"] = $"Deployment request {result.RequestNumber} submitted successfully.";
            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error creating request: " + ex.Message);
            await LoadDropdownsAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var request = await _requestService.GetByIdAsync(id);
        
        if (request == null)
            return NotFound();

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var (success, message) = await _requestService.CancelAsync(id, userId);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Details), new { id = id });
    }

    private async Task LoadDropdownsAsync()
    {
        ViewBag.Projects = new SelectList(await _masterDataService.GetProjectsAsync(), "ProjectId", "ProjectName");
        ViewBag.Environments = new SelectList(await _masterDataService.GetEnvironmentsAsync(), "EnvironmentId", "EnvironmentName");
        ViewBag.DeploymentTypes = new SelectList(await _masterDataService.GetDeploymentTypesAsync(), "DeploymentTypeId", "DeploymentTypeName");
    }
}
