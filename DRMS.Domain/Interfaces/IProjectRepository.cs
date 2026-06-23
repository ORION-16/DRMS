using DRMS.Domain.Entities;

namespace DRMS.Domain.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetProjectsAsync();
}
