using DRMS.Domain.Entities;

namespace DRMS.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> LoginAsync(string email, string passwordHash);
    Task UpdateLastLoginAsync(int userId);
}
