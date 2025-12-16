using AutoMapper;
using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.DTOs.Users;
using BilQalaam.Domain.Entities;
using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // 🟢 من Entity إلى DTO (للـ Get)
            CreateMap<ApplicationUser, UserResponseDto>();
            // 🟡 من Create DTO إلى Entity (للـ POST)
            CreateMap<CreateUserDto, ApplicationUser>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src =>
                    src.Role.HasValue ? src.Role.Value.ToString() : null))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                    src.Gender.HasValue ? src.Gender.Value.ToString() : null));

            // 🟠 من Update DTO إلى Entity (للـ PUT)
            CreateMap<UpdateUserDto, ApplicationUser>();
            // 🟢 Entity → Response DTO (GET)
            CreateMap<Lesson, LessonResponseDto>()
                .ForMember(dest => dest.StudentName,
                    opt => opt.MapFrom(src => src.Student != null ? src.Student.FullName : null))
                .ForMember(dest => dest.FamilyName,
                    opt => opt.MapFrom(src => src.Family != null ? src.Family.FullName : null));

            // 🟡 Create DTO → Entity (POST)
            CreateMap<CreateLessonDto, Lesson>();

            // 🟠 Update DTO → Entity (PUT)
            CreateMap<UpdateLessonDto, Lesson>();
                
        }
    }
}
