using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController(DataContext db) : BaseApiController
{
    [AllowAnonymous]
    [HttpGet]  // GET {domain}/api/users
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        var users = await db.Users.ToListAsync();
        return users;
    }

    [HttpGet("{id}")]   // GET {domain}/api/users/:id 
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        var user = await db.Users.FindAsync(id);

        if (user is null) return NotFound();

        return user;
    }
}

