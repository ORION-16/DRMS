namespace DRMS.Domain.Entities;

public class DeploymentHistory
{
    public int HistoryId { get; set; }
    public int DeploymentRequestId { get; set; }
    public int OldStatusId { get; set; }
    public int NewStatusId { get; set; }
    public int ActionBy { get; set; }
    public DateTime ActionDate { get; set; }
    public string? Remarks { get; set; }

    // Joined fields from stored procedures
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string ActionByName { get; set; } = string.Empty;
    public string ActionByRole { get; set; } = string.Empty;
}
