using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseApiController
{
    [HttpGet]  // GET {domain}/api/users
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }

    [HttpGet("{username}")]   // GET {domain}/api/users/:username 
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        bool isCurrentUser = User.GetUsername() == username;
        var user = await unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser);

        if (user is null) return NotFound();

        return user;
    }

    [HttpPut]   // PUT {domain}/api/users
    public async Task<ActionResult<MemberDto>> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.GetUsername();
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        if (user == null) return BadRequest("User not found");

        mapper.Map(memberUpdateDto, user);

        if (await unitOfWork.Complete())
        {
            var updatedMember = mapper.Map<MemberDto>(user);
            return updatedMember;
        }

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]  // POST {domain}/api/users/add-photo
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        // Get the current user from DB
        var username = User.GetUsername();
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        if (user == null) return BadRequest("User not found");
        // Upload the photo to Cloudinary
        var result = await photoService.AddPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error.Message);
        // Save the image data (coming from Cloudinary) into the current user (inside DB)
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        user.Photos.Add(photo);

        if (!await unitOfWork.Complete()) return BadRequest("Problem adding photo");

        // Return 201 response and add the location header for the created resource
        return CreatedAtAction(
            nameof(GetUser),  // As there is not a seperate api endpoint for images, we send the getuser api as location
            new { username = user.UserName },  // route parameters
            mapper.Map<PhotoDto>(photo)  // Convert the photo to a response dto object
        );
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest("User not found");

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null || photo.IsMain) return BadRequest("Can not use this as main photo");

        var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        if (await unitOfWork.Complete()) return NoContent();

        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest("User not found");

        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
        if (photo == null || photo.IsMain) return BadRequest("Main photo can not be deleted");
        // Delete from Cloudinary
        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }
        // Delete from DB
        user.Photos.Remove(photo);
        if (await unitOfWork.Complete()) return Ok();

        return BadRequest("Problem deleting photo");
    }
}

