using System.Security.Cryptography;
using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        // Add initial roles to DB
        var roles = new List<AppRole>
        {
            new() {Name = "Admin"},
            new() {Name = "Moderator"},
            new() {Name = "Member"},
        };
        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        // Add Other Users from json file to DB
        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);
        if (users is null) return;

        foreach (var user in users)
        {
            user.Photos.First().IsApproved = true;
            user.UserName = user.UserName!.ToLower();
            await userManager.CreateAsync(user, "Pa$$w0rd");
            await userManager.AddToRoleAsync(user, "Member");
        }

        // Add admin user to DB
        var admin = new AppUser
        {
            UserName = "admin",
            KnownAs = "Admin",
            Gender = "",
            City = "",
            Country = ""
        };
        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
    }
}
