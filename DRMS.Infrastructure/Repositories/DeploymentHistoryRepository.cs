using System.Data;
using Dapper;
using DRMS.Domain.Entities;
using DRMS.Domain.Interfaces;
using DRMS.Infrastructure.Data;

namespace DRMS.Infrastructure.Repositories;

public class DeploymentHistoryRepository : IDeploymentHistoryRepository
{
    private readonly DapperContext _context;

    public DeploymentHistoryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DeploymentHistory>> GetByRequestIdAsync(int deploymentRequestId)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DeploymentHistory>(
            "sp_GetDeploymentHistory",
            new { DeploymentRequestId = deploymentRequestId },
            commandType: CommandType.StoredProcedure);
    }
}
