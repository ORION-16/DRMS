using DRMS.Application.DTOs;
using DRMS.Application.Services.Interfaces;
using DRMS.Domain.Interfaces;

namespace DRMS.Application.Services.Implementations;

public class DeploymentRequestService : IDeploymentRequestService
{
    private readonly IDeploymentRequestRepository _requestRepository;

    public DeploymentRequestService(IDeploymentRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public async Task<(int DeploymentRequestId, string RequestNumber)> CreateAsync(CreateDeploymentRequestDto dto, int requestedBy)
    {
        return await _requestRepository.CreateAsync(
            dto.ProjectId, requestedBy, dto.SourceBranch,
            dto.TargetEnvironmentId, dto.DeploymentTypeId, dto.BuildVersion,
            dto.ChangeSummary, dto.RollbackPlan, dto.RequestedDeploymentDate);
    }

    public async Task<IEnumerable<DeploymentRequestDto>> GetByUserAsync(int userId)
    {
        var requests = await _requestRepository.GetByUserAsync(userId);
        return requests.Select(r => new DeploymentRequestDto
        {
            DeploymentRequestId = r.DeploymentRequestId,
            RequestNumber = r.RequestNumber,
            ProjectName = r.ProjectName,
            ProjectCode = r.ProjectCode,
            EnvironmentName = r.EnvironmentName,
            DeploymentTypeName = r.DeploymentTypeName,
            SourceBranch = r.SourceBranch,
            BuildVersion = r.BuildVersion,
            RequestedDeploymentDate = r.RequestedDeploymentDate,
            RequestedDate = r.RequestedDate.AddMinutes(330),
            CurrentStatus = r.CurrentStatus,
            CurrentApprovalLevel = r.CurrentApprovalLevel,
            IsActive = r.IsActive
        });
    }

    public async Task<DeploymentRequestDto?> GetByIdAsync(int deploymentRequestId)
    {
        var (request, approvals) = await _requestRepository.GetByIdAsync(deploymentRequestId);
        if (request == null) return null;

        return new DeploymentRequestDto
        {
            DeploymentRequestId = request.DeploymentRequestId,
            RequestNumber = request.RequestNumber,
            ProjectName = request.ProjectName,
            ProjectCode = request.ProjectCode,
            EnvironmentName = request.EnvironmentName,
            DeploymentTypeName = request.DeploymentTypeName,
            SourceBranch = request.SourceBranch,
            BuildVersion = request.BuildVersion,
            ChangeSummary = request.ChangeSummary,
            RollbackPlan = request.RollbackPlan,
            RequestedDeploymentDate = request.RequestedDeploymentDate,
            RequestedDate = request.RequestedDate.AddMinutes(330),
            CurrentStatus = request.CurrentStatus,
            CurrentApprovalLevel = request.CurrentApprovalLevel,
            RequestedByName = request.RequestedByName,
            EmployeeCode = request.EmployeeCode,
            Approvals = approvals.Select(a => new DeploymentApprovalDto
            {
                ApprovalLevel = a.ApprovalLevel,
                ActionTaken = a.ActionTaken,
                Remarks = a.Remarks,
                ActionDate = a.ActionDate.AddMinutes(330),
                ApproverName = a.ApproverName,
                ApproverRole = a.ApproverRole
            }).ToList()
        };
    }

    public async Task<(bool Success, string Message)> CancelAsync(int deploymentRequestId, int requestedBy)
    {
        return await _requestRepository.CancelAsync(deploymentRequestId, requestedBy);
    }

    public async Task<IEnumerable<DeploymentRequestDto>> GetAllAsync(int? statusId, int? projectId)
    {
        var requests = await _requestRepository.GetAllAsync(statusId, projectId);
        return requests.Select(r => new DeploymentRequestDto
        {
            DeploymentRequestId = r.DeploymentRequestId,
            RequestNumber = r.RequestNumber,
            ProjectName = r.ProjectName,
            EnvironmentName = r.EnvironmentName,
            DeploymentTypeName = r.DeploymentTypeName,
            SourceBranch = r.SourceBranch,
            RequestedDeploymentDate = r.RequestedDeploymentDate,
            RequestedDate = r.RequestedDate.AddMinutes(330),
            CurrentStatus = r.CurrentStatus,
            CurrentApprovalLevel = r.CurrentApprovalLevel,
            RequestedByName = r.RequestedByName
        });
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int userId, int roleId)
    {
        var summary = await _requestRepository.GetDashboardSummaryAsync(userId, roleId);
        return new DashboardSummaryDto
        {
            PendingCount = summary.PendingCount,
            TechLeadApprovedCount = summary.TechLeadApprovedCount,
            QAApprovedCount = summary.QAApprovedCount,
            DeployedCount = summary.DeployedCount,
            RejectedCount = summary.RejectedCount,
            RolledBackCount = summary.RolledBackCount,
            TotalCount = summary.TotalCount
        };
    }
}
