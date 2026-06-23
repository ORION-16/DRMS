namespace DRMS.Domain.Entities;

public class ApprovalWorkflow
{
    public int WorkflowId { get; set; }
    public int RoleId { get; set; }
    public int ApprovalOrder { get; set; }
    public bool IsActive { get; set; }
}
