using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(destination => destination.Age, options =>
                options.MapFrom(source =>
                    source.DateOfBirth.CalculateAge()))
            .ForMember(d => d.PhotoUrl, o =>
                o.MapFrom(s =>
                    s.Photos.FirstOrDefault(p => p.IsMain)!.Url));
        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
    }
}
