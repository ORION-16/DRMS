using DRMS.Application.DTOs;

namespace DRMS.Application.Services.Interfaces;

public interface IDeploymentRequestService
{
    Task<(int DeploymentRequestId, string RequestNumber)> CreateAsync(CreateDeploymentRequestDto dto, int requestedBy);
    Task<IEnumerable<DeploymentRequestDto>> GetByUserAsync(int userId);
    Task<DeploymentRequestDto?> GetByIdAsync(int deploymentRequestId);
    Task<(bool Success, string Message)> CancelAsync(int deploymentRequestId, int requestedBy);
    Task<IEnumerable<DeploymentRequestDto>> GetAllAsync(int? statusId, int? projectId);
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(int userId, int roleId);
}
