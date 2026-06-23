namespace DRMS.Domain.Entities;

public class Environment
{
    public int EnvironmentId { get; set; }
    public string EnvironmentName { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public bool IsActive { get; set; }
}
