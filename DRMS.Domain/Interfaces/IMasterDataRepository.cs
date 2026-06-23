using DRMS.Domain.Entities;
using Environment = DRMS.Domain.Entities.Environment;

namespace DRMS.Domain.Interfaces;

public interface IMasterDataRepository
{
    Task<IEnumerable<Project>> GetProjectsAsync();
    Task<IEnumerable<Environment>> GetEnvironmentsAsync();
    Task<IEnumerable<DeploymentType>> GetDeploymentTypesAsync();
    Task<IEnumerable<StatusMaster>> GetStatusesAsync();
}
