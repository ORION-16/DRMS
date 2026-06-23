namespace DRMS.Application.DTOs;

public class DashboardSummaryDto
{
    public int PendingCount { get; set; }
    public int TechLeadApprovedCount { get; set; }
    public int QAApprovedCount { get; set; }
    public int DeployedCount { get; set; }
    public int RejectedCount { get; set; }
    public int RolledBackCount { get; set; }
    public int TotalCount { get; set; }
}
