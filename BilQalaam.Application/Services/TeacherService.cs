using AutoMapper;
using BilQalaam.Application.DTOs.Teachers;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using BilQalaam.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace BilQalaam.Application.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TeacherService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetAllAsync()
        {
            var teachers = await _unitOfWork
                .Repository<Teacher>()
                .GetAllAsync();

            return _mapper.Map<IEnumerable<TeacherResponseDto>>(teachers);
        }

        public async Task<TeacherResponseDto?> GetByIdAsync(int id)
        {
            var teacher = await _unitOfWork
                .Repository<Teacher>()
                .GetByIdAsync(id);

            return teacher == null ? null : _mapper.Map<TeacherResponseDto>(teacher);
        }

        public async Task<int> CreateAsync(CreateTeacherDto dto, string createdByUserId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1?? ≈‰‘«¡ «·ÌÊ“— √Ê·«
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    FullName = dto.FullName,
                    Role = UserRole.Teacher,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    throw new ValidationException(errors);
                }

                // ≈÷«›… Role
                const string roleName = "Teacher";
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new IdentityRole(roleName));

                await _userManager.AddToRoleAsync(user, roleName);

                // 2?? ≈‰‘«¡ «·„⁄·„ „— »ÿ »«·ÌÊ“—
                var teacher = new Teacher
                {
                    TeacherName = dto.TeacherName,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    SupervisorId = dto.SupervisorId,
                    HourlyRate = dto.HourlyRate,
                    Currency = dto.Currency,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                };

                await _unitOfWork.Repository<Teacher>().AddAsync(teacher);
                await _unitOfWork.CompleteAsync();

                await transaction.CommitAsync();

                return teacher.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int id, UpdateTeacherDto dto, string updatedByUserId)
        {
            var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(id);
            if (teacher == null) return false;

            //  ÕœÌÀ »Ì«‰«  «·„⁄·„
            teacher.TeacherName = dto.TeacherName;
            teacher.PhoneNumber = dto.PhoneNumber;
            teacher.SupervisorId = dto.SupervisorId;
            teacher.HourlyRate = dto.HourlyRate;
            teacher.Currency = dto.Currency;
            teacher.UpdatedAt = DateTime.UtcNow;
            teacher.UpdatedBy = updatedByUserId;

            //  ÕœÌÀ »Ì«‰«  «·ÌÊ“—
            var user = await _userManager.FindByIdAsync(teacher.UserId);
            if (user != null)
            {
                user.FullName = dto.FullName;
                user.PhoneNumber = dto.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            _unitOfWork.Repository<Teacher>().Update(teacher);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(id);
            if (teacher == null) return false;

            // Õ–› «·ÌÊ“—
            var user = await _userManager.FindByIdAsync(teacher.UserId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            _unitOfWork.Repository<Teacher>().Delete(teacher);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
