using DRMS.Domain.Entities;

namespace DRMS.Domain.Interfaces;

public interface IDeploymentHistoryRepository
{
    Task<IEnumerable<DeploymentHistory>> GetByRequestIdAsync(int deploymentRequestId);
}
