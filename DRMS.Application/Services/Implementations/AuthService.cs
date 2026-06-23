using DRMS.Application.DTOs;
using DRMS.Application.Services.Interfaces;
using DRMS.Domain.Interfaces;

namespace DRMS.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> LoginAsync(LoginDto loginDto)
    {
        // 1. Hash the incoming password with our fixed salt
        string fixedSalt = "$2a$11$1234567890123456789012";
        string hashedInput = BCrypt.Net.BCrypt.HashPassword(loginDto.Password, fixedSalt);

        // 2. Call the repository which uses the sp_LoginUser SP
        var user = await _userRepository.LoginAsync(loginDto.Email, hashedInput);

        if (user == null) return null;

        // 3. Update last login date
        await _userRepository.UpdateLastLoginAsync(user.UserId);

        // 4. Return DTO
        return new UserDto
        {
            UserId = user.UserId,
            EmployeeCode = user.EmployeeCode,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.RoleName,
            IsActive = user.IsActive
        };
    }
}
