using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Constants;

namespace AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly RoleManager<ApplicationRole> _roles;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<ApplicationUser> users,
                           SignInManager<ApplicationUser> signIn,
                           RoleManager<ApplicationRole> roles,
                           ITokenService tokenService)
        {
            _users = users; _signIn = signIn; _roles = roles; _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName
            };

            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!await _roles.RoleExistsAsync(Roles.Tenant))
                await _roles.CreateAsync(new ApplicationRole { Name = Roles.Tenant });

            await _users.AddToRoleAsync(user, Roles.Tenant);

            var userRoles = await _users.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user, userRoles);

            return new AuthResponseDto(token, user.Id.ToString(), user.Email!, userRoles);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
        {
            var user = await _users.FindByEmailAsync(dto.Email)
                       ?? throw new Exception("Invalid credentials.");

            var valid = await _signIn.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!valid.Succeeded) throw new Exception("Invalid credentials.");

            var roles = await _users.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user, roles);

            return new AuthResponseDto(token, user.Id.ToString(), user.Email!, roles);
        }

        public async Task<bool> AssignRoleAsync(AssignRoleDto dto)
        {
            var user = await _users.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("User not found");

            // Check if role exists in DB first
            var roleExists = await _roles.RoleExistsAsync(dto.Role);
            if (!roleExists) throw new Exception($"Role '{dto.Role}' does not exist in the system.");

            var result = await _users.AddToRoleAsync(user, dto.Role);
            if (!result.Succeeded)
            {
                // Capture the specific Identity error (e.g., "User already in role")
                var error = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Identity Error: {error}");
            }

            return true;
        }

        public async Task<bool> RemoveRoleAsync(AssignRoleDto dto)
        {
            var user = await _users.FindByEmailAsync(dto.Email);
            if (user == null) throw new Exception("User not found");

            // Check if the user actually has this role before trying to remove it
            var hasRole = await _users.IsInRoleAsync(user, dto.Role);
            if (!hasRole) throw new Exception("User does not have this role");

            var result = await _users.RemoveFromRoleAsync(user, dto.Role);
            if (!result.Succeeded)
            {
                var error = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to remove role: {error}");
            }

            return true;
        }


        public async Task<UserInfoDto> GetUserInfoAsync(string userId)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var roles = await _users.GetRolesAsync(user);

            return new UserInfoDto(
                user.Id.ToString(),
                user.Email!,
                roles
            );
        }


    }
}
