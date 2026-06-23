namespace DRMS.Domain.Entities;

public class DeploymentApproval
{
    public int ApprovalId { get; set; }
    public int DeploymentRequestId { get; set; }
    public int ApproverId { get; set; }
    public int ApprovalLevel { get; set; }
    public string ActionTaken { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime ActionDate { get; set; }

    // Joined fields from stored procedures
    public string ApproverName { get; set; } = string.Empty;
    public string ApproverRole { get; set; } = string.Empty;
}
