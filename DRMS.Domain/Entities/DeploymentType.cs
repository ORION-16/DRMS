namespace DRMS.Domain.Entities;

public class DeploymentType
{
    public int DeploymentTypeId { get; set; }
    public string DeploymentTypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
