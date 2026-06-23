using DRMS.Application.DTOs;

namespace DRMS.Application.Services.Interfaces;

public interface IApprovalService
{
    Task<IEnumerable<DeploymentRequestDto>> GetPendingApprovalsAsync(int approverId);
    Task<(bool Success, string Message)> ProcessApprovalAsync(ProcessApprovalDto dto, int approverId);
}
