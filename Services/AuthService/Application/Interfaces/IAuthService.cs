using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);
        Task<AuthResponseDto> LoginAsync(LoginUserDto dto);

        Task<bool> AssignRoleAsync(AssignRoleDto dto);

        Task<UserInfoDto> GetUserInfoAsync(string userId);


    }
}
