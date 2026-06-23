using DRMS.Domain.Entities;

namespace DRMS.Domain.Interfaces;

public interface IDeploymentApprovalRepository
{
    Task<IEnumerable<DeploymentRequest>> GetPendingApprovalsAsync(int approverId);
    Task<(bool Success, string Message)> ProcessApprovalAsync(int deploymentRequestId, int approverId, string actionTaken, string remarks);
}
