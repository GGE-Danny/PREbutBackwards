using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public string? FullName { get; set; }
    }
}
