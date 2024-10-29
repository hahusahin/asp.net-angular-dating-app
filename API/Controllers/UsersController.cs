using System.Security.Claims;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpGet]  // GET {domain}/api/users
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await userRepository.GetMembersAsync();
        return Ok(users);
    }

    [HttpGet("{username}")]   // GET {domain}/api/users/:username 
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await userRepository.GetMemberAsync(username);

        if (user is null) return NotFound();

        return user;
    }

    [HttpPut]   // PUT {domain}/api/users
    public async Task<ActionResult<MemberDto>> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (username == null) return BadRequest("No username found in token");

        var user = await userRepository.GetUserByUsernameAsync(username);

        if (user == null) return BadRequest("User not found");

        mapper.Map(memberUpdateDto, user);

        if (await userRepository.SaveAllAsync())
        {
            var updatedMember = mapper.Map<MemberDto>(user);
            return updatedMember;
        }

        return BadRequest("Failed to update user");
    }
}

