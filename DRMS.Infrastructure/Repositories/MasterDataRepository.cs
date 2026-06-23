using System.Data;
using Dapper;
using DRMS.Domain.Entities;
using DRMS.Domain.Interfaces;
using DRMS.Infrastructure.Data;
using Environment = DRMS.Domain.Entities.Environment;

namespace DRMS.Infrastructure.Repositories;

public class MasterDataRepository : IMasterDataRepository
{
    private readonly DapperContext _context;

    public MasterDataRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetProjectsAsync()
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Project>(
            "sp_GetProjects",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Environment>> GetEnvironmentsAsync()
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Environment>(
            "sp_GetEnvironments",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<DeploymentType>> GetDeploymentTypesAsync()
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DeploymentType>(
            "sp_GetDeploymentTypes",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<StatusMaster>> GetStatusesAsync()
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<StatusMaster>(
            "sp_GetStatuses",
            commandType: CommandType.StoredProcedure);
    }
}
