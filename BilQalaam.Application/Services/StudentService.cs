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

        public async Task<(IEnumerable<StudentResponseDto>, int)> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var students = await _unitOfWork
                    .Repository<Student>()
                    .GetAllAsync();

                var totalCount = students.Count();
                var paginatedStudents = students
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return (_mapper.Map<IEnumerable<StudentResponseDto>>(paginatedStudents), totalCount);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì Ã·» «·ÿ·«»: {ex.Message}" });
            }
        }

        public async Task<StudentResponseDto?> GetByIdAsync(int id)
        {
            try
            {
                var student = await _unitOfWork
                    .Repository<Student>()
                    .GetByIdAsync(id);

                return student == null ? null : _mapper.Map<StudentResponseDto>(student);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì Ã·» «·ÿ«·»: {ex.Message}" });
            }
        }

        public async Task<IEnumerable<StudentResponseDto>> GetByFamilyIdAsync(int familyId)
        {
            try
            {
                var students = await _unitOfWork
                    .Repository<Student>()
                    .FindAsync(s => s.FamilyId == familyId);

                return _mapper.Map<IEnumerable<StudentResponseDto>>(students);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì Ã·» ÿ·«» «·⁄«∆·…: {ex.Message}" });
            }
        }

        public async Task<IEnumerable<StudentResponseDto>> GetByTeacherIdAsync(int teacherId)
        {
            try
            {
                var students = await _unitOfWork
                    .Repository<Student>()
                    .FindAsync(s => s.TeacherId == teacherId);

                return _mapper.Map<IEnumerable<StudentResponseDto>>(students);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì Ã·» ÿ·«» «·„⁄·„: {ex.Message}" });
            }
        }

        public async Task<int> CreateAsync(CreateStudentDto dto, string createdByUserId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // «· Õﬁﬁ „‰ ⁄œ„  ﬂ—«— «·«Ì„Ì·
                var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUserByEmail != null)
                    throw new ValidationException(new List<string> { "«·»—Ìœ «·≈·ﬂ —Ê‰Ì „” Œœ„ »«·›⁄·" });

                // «· Õﬁﬁ „‰ ⁄œ„  ﬂ—«— «”„ «·ÿ«·»
                var students = await _unitOfWork.Repository<Student>().FindAsync(s => s.StudentName == dto.StudentName);
                if (students.Any())
                    throw new ValidationException(new List<string> { "«”„ «·ÿ«·» „” Œœ„ »«·›⁄·" });

                // «· Õﬁﬁ „‰ ÊÃÊœ «·⁄«∆·…
                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(dto.FamilyId);
                if (family == null)
                    throw new ValidationException(new List<string> { "«·√”—… €Ì— „ÊÃÊœ…" });

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
            catch (ValidationException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì ≈‰‘«¡ «·ÿ«·»: {ex.Message}" });
            }
        }

        public async Task<bool> UpdateAsync(int id, UpdateStudentDto dto, string updatedByUserId)
        {
            try
            {
                var student = await _unitOfWork.Repository<Student>().GetByIdAsync(id);
                if (student == null) return false;

                // «· Õﬁﬁ „‰ ⁄œ„  ﬂ—«— «”„ «·ÿ«·» (≈–«  „  €ÌÌ—Â)
                if (student.StudentName != dto.StudentName)
                {
                    var existingStudent = await _unitOfWork.Repository<Student>()
                        .FindAsync(s => s.StudentName == dto.StudentName && s.Id != id);
                    if (existingStudent.Any())
                        throw new ValidationException(new List<string> { "«”„ «·ÿ«·» „” Œœ„ »«·›⁄·" });
                }

                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(dto.FamilyId);
                if (family == null) return false;

                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(dto.TeacherId);
                if (teacher == null) return false;

                student.StudentName = dto.StudentName;
                student.PhoneNumber = dto.PhoneNumber;
                student.HourlyRate = dto.HourlyRate;
                student.Currency = dto.Currency;
                student.FamilyId = dto.FamilyId;
                student.TeacherId = dto.TeacherId;
                student.UpdatedAt = DateTime.UtcNow;
                student.UpdatedBy = updatedByUserId;

                var user = await _userManager.FindByIdAsync(student.UserId);
                if (user != null)
                {
                    user.FullName = dto.FullName;
                    user.PhoneNumber = dto.PhoneNumber;
                    user.UpdatedAt = DateTime.UtcNow;

                    //  ÕœÌÀ ﬂ·„… «·„—Ê— ≈–«  „  Ê›Ì—Â«
                    if (!string.IsNullOrWhiteSpace(dto.Password))
                    {
                        var removeResult = await _userManager.RemovePasswordAsync(user);
                        if (!removeResult.Succeeded)
                        {
                            var errors = removeResult.Errors.Select(e => e.Description).ToList();
                            throw new ValidationException(errors);
                        }

                        var addResult = await _userManager.AddPasswordAsync(user, dto.Password);
                        if (!addResult.Succeeded)
                        {
                            var errors = addResult.Errors.Select(e => e.Description).ToList();
                            throw new ValidationException(errors);
                        }
                    }

                    await _userManager.UpdateAsync(user);
                }

                _unitOfWork.Repository<Student>().Update(student);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (InvalidOperationException ex) when (ex.InnerException is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ValidationException(new List<string> { "«·»Ì«‰«   „  ⁄œÌ·Â« „‰ ﬁ»· „” Œœ„ ¬Œ—. Ì—ÃÏ ≈⁄«œ… «·„Õ«Ê·…." });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ValidationException(new List<string> { "«·»Ì«‰«   „  ⁄œÌ·Â« „‰ ﬁ»· „” Œœ„ ¬Œ—. Ì—ÃÏ ≈⁄«œ… «·„Õ«Ê·…." });
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì  ÕœÌÀ «·ÿ«·»: {ex.Message}" });
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var student = await _unitOfWork.Repository<Student>().GetByIdAsync(id);
                if (student == null) return false;

                // ??? Soft Delete
                student.IsDeleted = true;
                student.DeletedAt = DateTime.UtcNow;
                student.DeletedBy = id.ToString();

                _unitOfWork.Repository<Student>().Update(student);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (InvalidOperationException ex) when (ex.InnerException is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ValidationException(new List<string> { "«·»Ì«‰«   „  ⁄œÌ·Â« „‰ ﬁ»· „” Œœ„ ¬Œ—. Ì—ÃÏ ≈⁄«œ… «·„Õ«Ê·…." });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ValidationException(new List<string> { "«·»Ì«‰«   „  ⁄œÌ·Â« „‰ ﬁ»· „” Œœ„ ¬Œ—. Ì—ÃÏ ≈⁄«œ… «·„Õ«Ê·…." });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
            {
                throw new ValidationException(new List<string> { "·« Ì„ﬂ‰ Õ–› Â–« «·ÿ«·» ·√‰Â „— »ÿ »»Ì«‰«  √Œ—Ï." });
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì Õ–› «·ÿ«·»: {ex.Message}" });
            }
        }
    }
}
