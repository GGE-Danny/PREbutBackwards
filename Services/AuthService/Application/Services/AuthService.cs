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
    }
}
