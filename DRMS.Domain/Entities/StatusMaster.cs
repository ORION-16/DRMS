namespace DRMS.Domain.Entities;

public class StatusMaster
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
