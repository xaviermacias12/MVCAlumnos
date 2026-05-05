using Microsoft.AspNetCore.Identity;

namespace MVCAlumnos.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool DebeCambiarPassword { get; set; } = false;
    }
}