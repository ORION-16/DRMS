using DRMS.Application.DTOs;
using DRMS.Application.Services.Interfaces;
using DRMS.Domain.Interfaces;

namespace DRMS.Application.Services.Implementations;

public class MasterDataService : IMasterDataService
{
    private readonly IMasterDataRepository _masterDataRepository;

    public MasterDataService(IMasterDataRepository masterDataRepository)
    {
        _masterDataRepository = masterDataRepository;
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsAsync()
    {
        var projects = await _masterDataRepository.GetProjectsAsync();
        return projects.Select(p => new ProjectDto
        {
            ProjectId = p.ProjectId,
            ProjectCode = p.ProjectCode,
            ProjectName = p.ProjectName
        });
    }

    public async Task<IEnumerable<EnvironmentDto>> GetEnvironmentsAsync()
    {
        var environments = await _masterDataRepository.GetEnvironmentsAsync();
        return environments.Select(e => new EnvironmentDto
        {
            EnvironmentId = e.EnvironmentId,
            EnvironmentName = e.EnvironmentName,
            SequenceOrder = e.SequenceOrder
        });
    }

    public async Task<IEnumerable<DeploymentTypeDto>> GetDeploymentTypesAsync()
    {
        var types = await _masterDataRepository.GetDeploymentTypesAsync();
        return types.Select(t => new DeploymentTypeDto
        {
            DeploymentTypeId = t.DeploymentTypeId,
            DeploymentTypeName = t.DeploymentTypeName
        });
    }

    public async Task<IEnumerable<StatusDto>> GetStatusesAsync()
    {
        var statuses = await _masterDataRepository.GetStatusesAsync();
        return statuses.Select(s => new StatusDto
        {
            StatusId = s.StatusId,
            StatusName = s.StatusName
        });
    }
}
