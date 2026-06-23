using DRMS.Application.DTOs;

namespace DRMS.Application.Services.Interfaces;

public interface IMasterDataService
{
    Task<IEnumerable<ProjectDto>> GetProjectsAsync();
    Task<IEnumerable<EnvironmentDto>> GetEnvironmentsAsync();
    Task<IEnumerable<DeploymentTypeDto>> GetDeploymentTypesAsync();
    Task<IEnumerable<StatusDto>> GetStatusesAsync();
}

public class EnvironmentDto
{
    public int EnvironmentId { get; set; }
    public string EnvironmentName { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
}

public class DeploymentTypeDto
{
    public int DeploymentTypeId { get; set; }
    public string DeploymentTypeName { get; set; } = string.Empty;
}

public class StatusDto
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
}
