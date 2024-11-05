using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController(ILikesRepository likesRepository) : BaseApiController
    {
        [HttpPost("{targetUserId:int}")] // POST {domain}/api/likes/:targetUserId 
        public async Task<ActionResult> ToggleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();
            if (sourceUserId == targetUserId) return BadRequest("You can not like yourself");

            var existingLike = await likesRepository.GetUserLike(sourceUserId, targetUserId);
            // I haven't liked the target user before, so add it 
            if (existingLike is null)
            {
                var newLike = new UserLike
                {
                    SourceUserId = sourceUserId,
                    TargetUserId = targetUserId
                };
                likesRepository.AddLike(newLike);
            }
            // I've liked before,so now delete it
            else
            {
                likesRepository.DeleteLike(existingLike);
            }

            if (await likesRepository.SaveChanges()) return Ok();

            return BadRequest("Failed to update like");
        }

        [HttpGet("list")]  // GET {domain}/api/likes/list
        public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
        {
            var ids = await likesRepository.GetCurrentUserLikeIds(User.GetUserId());
            return Ok(ids);
        }

        [HttpGet]  // GET {domain}/api/likes?predicate={liked/likedBy/mutual}
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users);
            return Ok(users);
        }
    }
}
