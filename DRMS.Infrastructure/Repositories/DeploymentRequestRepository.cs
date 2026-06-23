using System.Data;
using Dapper;
using DRMS.Domain.Entities;
using DRMS.Domain.Interfaces;
using DRMS.Infrastructure.Data;

namespace DRMS.Infrastructure.Repositories;

public class DeploymentRequestRepository : IDeploymentRequestRepository
{
    private readonly DapperContext _context;

    public DeploymentRequestRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<(int DeploymentRequestId, string RequestNumber)> CreateAsync(
        int projectId, int requestedBy, string sourceBranch,
        int targetEnvironmentId, int deploymentTypeId, string? buildVersion,
        string changeSummary, string rollbackPlan, DateTime requestedDeploymentDate)
    {
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstAsync<dynamic>(
            "sp_CreateDeploymentRequest",
            new
            {
                ProjectId = projectId,
                RequestedBy = requestedBy,
                SourceBranch = sourceBranch,
                TargetEnvironmentId = targetEnvironmentId,
                DeploymentTypeId = deploymentTypeId,
                BuildVersion = buildVersion,
                ChangeSummary = changeSummary,
                RollbackPlan = rollbackPlan,
                RequestedDeploymentDate = requestedDeploymentDate
            },
            commandType: CommandType.StoredProcedure);

        return ((int)result.DeploymentRequestId, (string)result.RequestNumber);
    }

    public async Task<IEnumerable<DeploymentRequest>> GetByUserAsync(int userId)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DeploymentRequest>(
            "sp_GetRequestsByUser",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<(DeploymentRequest? Request, IEnumerable<DeploymentApproval> Approvals)> GetByIdAsync(int deploymentRequestId)
    {
        using var connection = _context.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            "sp_GetRequestById",
            new { DeploymentRequestId = deploymentRequestId },
            commandType: CommandType.StoredProcedure);

        var request = await multi.ReadFirstOrDefaultAsync<DeploymentRequest>();
        var approvals = await multi.ReadAsync<DeploymentApproval>();

        return (request, approvals);
    }

    public async Task<(bool Success, string Message)> CancelAsync(int deploymentRequestId, int requestedBy)
    {
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstAsync<dynamic>(
            "sp_CancelRequest",
            new { DeploymentRequestId = deploymentRequestId, RequestedBy = requestedBy },
            commandType: CommandType.StoredProcedure);

        return ((bool)(result.Success == 1), (string)result.Message);
    }

    public async Task<IEnumerable<DeploymentRequest>> GetAllAsync(int? statusId, int? projectId)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DeploymentRequest>(
            "sp_GetAllRequests",
            new { StatusId = statusId, ProjectId = projectId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync(int userId, int roleId)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstAsync<DashboardSummary>(
            "sp_GetDashboardSummary",
            new { UserId = userId, RoleId = roleId },
            commandType: CommandType.StoredProcedure);
    }
}
