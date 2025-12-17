using AutoMapper;
using BilQalaam.Application.DTOs.Students;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using BilQalaam.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace BilQalaam.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentService(
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

        public async Task<IEnumerable<StudentResponseDto>> GetAllAsync()
        {
            var students = await _unitOfWork
                .Repository<Student>()
                .GetAllAsync();

            return _mapper.Map<IEnumerable<StudentResponseDto>>(students);
        }

        public async Task<StudentResponseDto?> GetByIdAsync(int id)
        {
            var student = await _unitOfWork
                .Repository<Student>()
                .GetByIdAsync(id);

            return student == null ? null : _mapper.Map<StudentResponseDto>(student);
        }

        public async Task<IEnumerable<StudentResponseDto>> GetByFamilyIdAsync(int familyId)
        {
            var students = await _unitOfWork
                .Repository<Student>()
                .FindAsync(s => s.FamilyId == familyId);

            return _mapper.Map<IEnumerable<StudentResponseDto>>(students);
        }

        public async Task<IEnumerable<StudentResponseDto>> GetByTeacherIdAsync(int teacherId)
        {
            var students = await _unitOfWork
                .Repository<Student>()
                .FindAsync(s => s.TeacherId == teacherId);

            return _mapper.Map<IEnumerable<StudentResponseDto>>(students);
        }

        public async Task<int> CreateAsync(CreateStudentDto dto, string createdByUserId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // «· Õﬁﬁ „‰ ÊÃÊœ «·⁄«∆·…
                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(dto.FamilyId);
                if (family == null)
                    throw new ValidationException(new List<string> { "«·⁄«∆·… €Ì— „ÊÃÊœ…" });

                // «· Õﬁﬁ „‰ ÊÃÊœ «·„⁄·„
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(dto.TeacherId);
                if (teacher == null)
                    throw new ValidationException(new List<string> { "«·„⁄·„ €Ì— „ÊÃÊœ" });

                // 1?? ≈‰‘«¡ «·ÌÊ“— √Ê·«
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    FullName = dto.FullName,
                    Role = UserRole.Student,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    throw new ValidationException(errors);
                }

                // ≈÷«›… Role
                const string roleName = "Student";
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new IdentityRole(roleName));

                await _userManager.AddToRoleAsync(user, roleName);

                // 2?? ≈‰‘«¡ «·ÿ«·» „— »ÿ »«·ÌÊ“—
                var student = new Student
                {
                    StudentName = dto.StudentName,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    HourlyRate = dto.HourlyRate,
                    Currency = dto.Currency,
                    FamilyId = dto.FamilyId,
                    TeacherId = dto.TeacherId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                };

                await _unitOfWork.Repository<Student>().AddAsync(student);
                await _unitOfWork.CompleteAsync();

                await transaction.CommitAsync();

                return student.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int id, UpdateStudentDto dto, string updatedByUserId)
        {
            var student = await _unitOfWork.Repository<Student>().GetByIdAsync(id);
            if (student == null) return false;

            // «· Õﬁﬁ „‰ ÊÃÊœ «·⁄«∆·…
            var family = await _unitOfWork.Repository<Family>().GetByIdAsync(dto.FamilyId);
            if (family == null) return false;

            // «· Õﬁﬁ „‰ ÊÃÊœ «·„⁄·„
            var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(dto.TeacherId);
            if (teacher == null) return false;

            //  ÕœÌÀ »Ì«‰«  «·ÿ«·»
            student.StudentName = dto.StudentName;
            student.PhoneNumber = dto.PhoneNumber;
            student.HourlyRate = dto.HourlyRate;
            student.Currency = dto.Currency;
            student.FamilyId = dto.FamilyId;
            student.TeacherId = dto.TeacherId;
            student.UpdatedAt = DateTime.UtcNow;
            student.UpdatedBy = updatedByUserId;

            //  ÕœÌÀ »Ì«‰«  «·ÌÊ“—
            var user = await _userManager.FindByIdAsync(student.UserId);
            if (user != null)
            {
                user.FullName = dto.FullName;
                user.PhoneNumber = dto.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            _unitOfWork.Repository<Student>().Update(student);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var student = await _unitOfWork.Repository<Student>().GetByIdAsync(id);
            if (student == null) return false;

            // Õ–› «·ÌÊ“—
            var user = await _userManager.FindByIdAsync(student.UserId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            _unitOfWork.Repository<Student>().Delete(student);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
