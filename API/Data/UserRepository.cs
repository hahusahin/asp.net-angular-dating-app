using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
    public async Task<MemberDto?> GetMemberAsync(string username, bool isCurrentUser)
    {
        var query = context.Users
            .Where(u => u.UserName == username)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .AsQueryable();

        if (isCurrentUser) query = query.IgnoreQueryFilters();

        return await query.FirstOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query = context.Users.AsQueryable();

        query = query.Where(u => u.UserName != userParams.CurrentUsername);

        if (userParams.Gender != null) query = query.Where(u => u.Gender == userParams.Gender);

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));
        query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive)
        };

        return await PagedList<MemberDto>.CreateAsync(
            query.ProjectTo<MemberDto>(mapper.ConfigurationProvider),
            userParams.PageNumber,
            userParams.PageSize
        );
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return await context.Users
            .Include(u => u.Photos)
            .SingleOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await context.Users
            .Include(u => u.Photos)
            .ToListAsync();
    }

    public void Update(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }

    public async Task<AppUser?> GetUserByPhotoId(int photoId)
    {
        return await context.Users
            .Include(u => u.Photos)
            .IgnoreQueryFilters()
            .Where(u => u.Photos.Any(p => p.Id == photoId))
            .FirstOrDefaultAsync();
    }
}
