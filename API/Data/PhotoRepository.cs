using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository(DataContext context) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoById(int id)
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .Where(p => p.IsApproved == false)
            .Select(p => new PhotoForApprovalDto
            {
                Id = p.Id,
                Username = p.AppUser.UserName,
                Url = p.Url,
                IsApproved = p.IsApproved
            })
            .ToListAsync();
    }

    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}
