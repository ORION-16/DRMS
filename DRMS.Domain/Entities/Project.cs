namespace DRMS.Domain.Entities;

public class Project
{
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectDescription { get; set; }
    public int TechLeadId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
