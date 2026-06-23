using System.Data;
using Dapper;
using DRMS.Domain.Entities;
using DRMS.Domain.Interfaces;
using DRMS.Infrastructure.Data;

namespace DRMS.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly DapperContext _context;

    public ProjectRepository(DapperContext context)
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
}
