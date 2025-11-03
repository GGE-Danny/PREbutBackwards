namespace AuthService.Application.DTOs
{
    public record RegisterUserDto(string Email, string Password, string? FullName, string? PhoneNumber);
}
