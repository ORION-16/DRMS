using System.Data;
using Dapper;
using DRMS.Domain.Entities;
using DRMS.Domain.Interfaces;
using DRMS.Infrastructure.Data;

namespace DRMS.Infrastructure.Repositories;

public class DeploymentApprovalRepository : IDeploymentApprovalRepository
{
    private readonly DapperContext _context;

    public DeploymentApprovalRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DeploymentRequest>> GetPendingApprovalsAsync(int approverId)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DeploymentRequest>(
            "sp_GetPendingApprovalsForUser",
            new { ApproverId = approverId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<(bool Success, string Message)> ProcessApprovalAsync(
        int deploymentRequestId, int approverId, string actionTaken, string remarks)
    {
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstAsync<dynamic>(
            "sp_ProcessApproval",
            new
            {
                DeploymentRequestId = deploymentRequestId,
                ApproverId = approverId,
                ActionTaken = actionTaken,
                Remarks = remarks
            },
            commandType: CommandType.StoredProcedure);

        return ((bool)(result.Success == 1), (string)result.Message);
    }
}
