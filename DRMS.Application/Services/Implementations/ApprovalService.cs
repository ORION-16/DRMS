using DRMS.Application.DTOs;
using DRMS.Application.Services.Interfaces;
using DRMS.Domain.Interfaces;

namespace DRMS.Application.Services.Implementations;

public class ApprovalService : IApprovalService
{
    private readonly IDeploymentApprovalRepository _approvalRepository;

    public ApprovalService(IDeploymentApprovalRepository approvalRepository)
    {
        _approvalRepository = approvalRepository;
    }

    public async Task<IEnumerable<DeploymentRequestDto>> GetPendingApprovalsAsync(int approverId)
    {
        var requests = await _approvalRepository.GetPendingApprovalsAsync(approverId);
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
            RequestedByName = r.RequestedByName,
            CurrentApprovalLevel = r.CurrentApprovalLevel
        });
    }

    public async Task<(bool Success, string Message)> ProcessApprovalAsync(ProcessApprovalDto dto, int approverId)
    {
        return await _approvalRepository.ProcessApprovalAsync(
            dto.DeploymentRequestId, approverId, dto.ActionTaken, dto.Remarks ?? "");
    }
}
