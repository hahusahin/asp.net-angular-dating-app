using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext db, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]   // POST: /api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await IsUserExists(registerDto.Username)) return BadRequest("Username is taken");

        using var hmac = new HMACSHA512();
        var user = new AppUser
        {
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return new UserDto { Username = user.UserName, Token = tokenService.CreateToken(user) };
    }

    [HttpPost("login")]   // POST: /api/account/login
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == loginDto.Username.ToLower());
        if (user == null) return Unauthorized("Invalid username");

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        }

        return new UserDto { Username = user.UserName, Token = tokenService.CreateToken(user) };
    }

    private async Task<bool> IsUserExists(string username)
    {
        return await db.Users.AnyAsync(u => u.UserName.ToLower() == username.ToLower());
    }
}
