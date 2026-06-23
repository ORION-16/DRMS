namespace DRMS.Application.DTOs;

public class DeploymentApprovalDto
{
    public int ApprovalLevel { get; set; }
    public string ActionTaken { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime ActionDate { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string ApproverRole { get; set; } = string.Empty;
}
