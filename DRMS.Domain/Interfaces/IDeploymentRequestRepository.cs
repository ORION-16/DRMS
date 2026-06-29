using DRMS.Domain.Entities;

namespace DRMS.Domain.Interfaces;

public interface IDeploymentRequestRepository
{
    Task<(int DeploymentRequestId, string RequestNumber)> CreateAsync(
        int projectId, int requestedBy, string sourceBranch,
        int targetEnvironmentId, int deploymentTypeId, string? buildVersion,
        string changeSummary, string rollbackPlan, DateTime requestedDeploymentDate);

    Task<IEnumerable<DeploymentRequest>> GetByUserAsync(int userId);

    Task<(DeploymentRequest? Request, IEnumerable<DeploymentApproval> Approvals)> GetByIdAsync(int deploymentRequestId);

    Task<(bool Success, string Message)> CancelAsync(int deploymentRequestId, int requestedBy);

    Task<IEnumerable<DeploymentRequest>> GetAllAsync(int? statusId, int? projectId);

    Task<DashboardSummary> GetDashboardSummaryAsync(int userId, int roleId);

    /// <summary>
    /// Returns the highest SequenceOrder of Environments where this project
    /// has at least one request in 'Deployed' status. Returns 0 if the project
    /// has never reached a Deployed state in any environment.
    /// </summary>
    Task<int> GetMaxDeployedEnvironmentSequenceAsync(int projectId);
}

public class DashboardSummary
{
    public int PendingCount { get; set; }
    public int TechLeadApprovedCount { get; set; }
    public int QAApprovedCount { get; set; }
    public int DeployedCount { get; set; }
    public int RejectedCount { get; set; }
    public int RolledBackCount { get; set; }
    public int TotalCount { get; set; }
}
