using Byway.Application.DTOs.Auth;
using Byway.Core.DTOs;

namespace Byway.Application.Interfaces
{
    public interface IUserService
    {
        Task<TokenResponseDto> LoginAsync(LoginDto dto);
        Task<UserDto> RegisterAsync(RegisterUserDto dto);
    }
}
