namespace DRMS.Application.DTOs;

public class DeploymentHistoryDto
{
    public int HistoryId { get; set; }
    public DateTime ActionDate { get; set; }
    public string? Remarks { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string ActionByName { get; set; } = string.Empty;
    public string ActionByRole { get; set; } = string.Empty;
}
