using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DRMS.Application.Services.Interfaces;
using DRMS.Application.DTOs;
using System.Security.Claims;

namespace DRMS.Web.Controllers;

[Authorize(Policy = "Approvers")]
public class ApprovalController : Controller
{
    private readonly IApprovalService _approvalService;
    private readonly IDeploymentRequestService _requestService;

    public ApprovalController(IApprovalService approvalService, IDeploymentRequestService requestService)
    {
        _approvalService = approvalService;
        _requestService = requestService;
    }

    [HttpGet]
    public async Task<IActionResult> Pending()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var requests = await _approvalService.GetPendingApprovalsAsync(userId);
        return View(requests);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var request = await _requestService.GetByIdAsync(id);
        if (request == null)
            return NotFound();

        ViewBag.DeploymentRequestId = id;
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(ProcessApprovalDto model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid approval submission.";
            return RedirectToAction(nameof(Details), new { id = model.DeploymentRequestId });
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var (success, message) = await _approvalService.ProcessApprovalAsync(model, userId);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Pending));
        }
        else
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Details), new { id = model.DeploymentRequestId });
        }
    }
}
