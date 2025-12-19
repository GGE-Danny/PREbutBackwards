namespace AuthService.Application.DTOs
{
    public class UserInfoDto(
    string UserId,
    string Email,
    IEnumerable<string> Roles
);
    
}
