using System;
using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppRole : IdentityRole<int>
{
    // Navigation prop
    public ICollection<AppUserRole> UserRoles { get; set; } = [];
}
