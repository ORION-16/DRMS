namespace DRMS.Domain.Entities;

public class DeploymentRequest
{
    public int DeploymentRequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public int RequestedBy { get; set; }
    public string SourceBranch { get; set; } = string.Empty;
    public int TargetEnvironmentId { get; set; }
    public int DeploymentTypeId { get; set; }
    public string? BuildVersion { get; set; }
    public string ChangeSummary { get; set; } = string.Empty;
    public string RollbackPlan { get; set; } = string.Empty;
    public DateTime RequestedDeploymentDate { get; set; }
    public DateTime RequestedDate { get; set; }
    public int CurrentStatusId { get; set; }
    public int CurrentApprovalLevel { get; set; }
    public bool IsActive { get; set; }

    // Joined fields from stored procedures
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
    public string DeploymentTypeName { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
}
