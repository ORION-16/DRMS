using DRMS.Application.DTOs;

namespace DRMS.Application.Services.Interfaces;

public interface IAuthService
{
    Task<UserDto?> LoginAsync(LoginDto loginDto);
}
