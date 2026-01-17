using AutoMapper;
using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.DTOs.Students;
using BilQalaam.Application.DTOs.Supervisors;
using BilQalaam.Application.DTOs.Teachers;
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
            
            // 🟢 Lesson: Entity → Response DTO (GET)
            CreateMap<Lesson, LessonResponseDto>()
                .ForMember(dest => dest.StudentName,
                    opt => opt.MapFrom(src => src.Student != null ? src.Student.StudentName : null))
                .ForMember(dest => dest.TeacherName,
                    opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.TeacherName : null))
                .ForMember(dest => dest.FamilyName,
                    opt => opt.MapFrom(src => src.Family != null ? src.Family.FamilyName : null))
                .ForMember(dest => dest.SupervisorName,
                    opt => opt.MapFrom(src => src.Supervisor != null ? src.Supervisor.SupervisorName : string.Empty));

            // 🟡 Lesson: Create DTO → Entity (POST)
            CreateMap<CreateLessonDto, Lesson>();

            // 🟠 Lesson: Update DTO → Entity (PUT)
            CreateMap<UpdateLessonDto, Lesson>();

            // 🟢 Family: Entity → Response DTO (GET)
            CreateMap<Family, FamilyResponseDto>()
                .ForMember(dest => dest.SupervisorName,
                    opt => opt.MapFrom(src => src.Supervisor != null ? src.Supervisor.SupervisorName : null))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : src.Email))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : src.PhoneNumber));

            // 🟢 Teacher: Entity → Response DTO (GET)
            CreateMap<Teacher, TeacherResponseDto>()
                .ForMember(dest => dest.SupervisorName,
                    opt => opt.MapFrom(src => src.Supervisor != null ? src.Supervisor.SupervisorName : null))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : src.Email))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : src.PhoneNumber));

            // 🟢 Supervisor: Entity → Response DTO (GET)
            CreateMap<Supervisor, SupervisorResponseDto>()
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : src.Email))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : src.PhoneNumber));

            // 🟢 Student: Entity → Response DTO (GET)
            CreateMap<Student, StudentResponseDto>()
                .ForMember(dest => dest.FamilyName,
                    opt => opt.MapFrom(src => src.Family != null ? src.Family.FamilyName : null))
                .ForMember(dest => dest.TeacherName,
                    opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.TeacherName : null));
        }
    }
}
