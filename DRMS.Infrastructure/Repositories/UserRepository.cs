using System.Data;
using Dapper;
using DRMS.Domain.Entities;
using DRMS.Domain.Interfaces;
using DRMS.Infrastructure.Data;

namespace DRMS.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<User?> LoginAsync(string email, string passwordHash)
    {
        using var connection = _context.CreateConnection();
        var user = await connection.QueryFirstOrDefaultAsync<User>(
            "sp_LoginUser",
            new { Email = email, PasswordHash = passwordHash },
            commandType: CommandType.StoredProcedure);
        return user;
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(
            "sp_UpdateLastLogin",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
    }
}
