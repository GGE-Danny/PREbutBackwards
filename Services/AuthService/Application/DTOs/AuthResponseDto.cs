namespace AuthService.Application.DTOs
{
    public record AuthResponseDto(string Token, string UserId, string Email, IEnumerable<string> Roles);
}
