namespace DRMS.Application.DTOs;

public class DeploymentRequestDto
{
    public int DeploymentRequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
    public string DeploymentTypeName { get; set; } = string.Empty;
    public string SourceBranch { get; set; } = string.Empty;
    public string? BuildVersion { get; set; }
    public string ChangeSummary { get; set; } = string.Empty;
    public string RollbackPlan { get; set; } = string.Empty;
    public DateTime RequestedDeploymentDate { get; set; }
    public DateTime RequestedDate { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public int CurrentApprovalLevel { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Approval timeline for detail view
    public List<DeploymentApprovalDto> Approvals { get; set; } = new();
}
