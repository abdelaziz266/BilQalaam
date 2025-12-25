using AutoMapper;
using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.DTOs.Teachers;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using BilQalaam.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        public async Task<(IEnumerable<TeacherResponseDto>, int)> GetAllAsync(string currentUserId, string role, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                List<Teacher> teachers = new();
                if (role == "Admin")
                {

                    var supervisor = await _unitOfWork.Repository<Supervisor>().Query().Where(s => s.UserId == currentUserId).FirstOrDefaultAsync();

                    teachers = await _unitOfWork
                        .Repository<Teacher>().Query().Where(x => x.SupervisorId == supervisor.Id).Include(x => x.Supervisor).ToListAsync();

                }
                else
                {
                    teachers = await _unitOfWork
                                     .Repository<Teacher>()
                                     .Query()
                                     .Include(t => t.Supervisor)
                                     .ToListAsync();
                }
                var totalCount = teachers.Count();
                var paginatedTeachers = teachers
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return (_mapper.Map<IEnumerable<TeacherResponseDto>>(paginatedTeachers), totalCount);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì Ã·» «·„⁄·„Ì‰: {ex.Message}" });
            }
        }

        public async Task<TeacherResponseDto?> GetByIdAsync(int id)
        {
            try
            {
                var teacher = await _unitOfWork
                    .Repository<Teacher>()
                    .GetByIdAsync(id);

                if (teacher != null && teacher.SupervisorId.HasValue)
                {
                    var supervisor = await _unitOfWork.Repository<Supervisor>().GetByIdAsync(teacher.SupervisorId.Value);
                    teacher.Supervisor = supervisor;
                }

                return teacher == null ? null : _mapper.Map<TeacherResponseDto>(teacher);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì Ã·» «·„⁄·„: {ex.Message}" });
            }
        }

        public async Task<(IEnumerable<TeacherResponseDto>, int)> GetBySupervisorIdsAsync(
        IEnumerable<int> supervisorIds,
        int pageNumber,
        int pageSize)
        {
            try
            {
                var query = _unitOfWork
                    .Repository<Teacher>()
                    .Query()
                    .Include(t => t.Supervisor)
                    .Where(t => t.SupervisorId.HasValue &&
                                supervisorIds.Contains(t.SupervisorId.Value));
                var totalCount = await query.CountAsync();

                var teachers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (_mapper.Map<IEnumerable<TeacherResponseDto>>(teachers), totalCount);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì Ã·» „⁄·„Ì «·„‘—›Ì‰: {ex.Message}" });
            }
        }

        public async Task<int> CreateAsync(CreateTeacherDto dto, string createdByUserId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // «· Õﬁﬁ „‰ ⁄œ„  ﬂ—«— «·«Ì„Ì·
                var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUserByEmail != null)
                    throw new ValidationException(new List<string> { "«·»—Ìœ «·≈·ﬂ —Ê‰Ì „” Œœ„ »«·›⁄·" });

                // «· Õﬁﬁ „‰ ⁄œ„  ﬂ—«— «”„ «·„⁄·„
                var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.TeacherName == dto.FullName);
                if (teachers.Any())
                    throw new ValidationException(new List<string> { "«”„ «·„⁄·„ „” Œœ„ »«·›⁄·" });

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

                const string roleName = "Teacher";
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new IdentityRole(roleName));

                await _userManager.AddToRoleAsync(user, roleName);

                var teacher = new Teacher
                {
                    TeacherName = dto.FullName,
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
            catch (ValidationException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì ≈‰‘«¡ «·„⁄·„: {ex.Message}" });
            }
        }

        public async Task<bool> UpdateAsync(int id, UpdateTeacherDto dto, string updatedByUserId)
        {
            try
            {
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(id);
                if (teacher == null) return false;

                // «· Õﬁﬁ „‰ ⁄œ„  ﬂ—«— «”„ «·„⁄·„ (≈–«  „  €ÌÌ—Â)
                if (teacher.TeacherName != dto.FullName)
                {
                    var existingTeacher = await _unitOfWork.Repository<Teacher>()
                        .FindAsync(t => t.TeacherName == dto.FullName && t.Id != id);
                    if (existingTeacher.Any())
                        throw new ValidationException(new List<string> { "«”„ «·„⁄·„ „” Œœ„ »«·›⁄·" });
                }

                teacher.TeacherName = dto.FullName;
                teacher.PhoneNumber = dto.PhoneNumber;
                teacher.SupervisorId = dto.SupervisorId;
                teacher.HourlyRate = dto.HourlyRate;
                teacher.Currency = dto.Currency;
                teacher.UpdatedAt = DateTime.UtcNow;
                teacher.UpdatedBy = updatedByUserId;

                var user = await _userManager.FindByIdAsync(teacher.UserId);
                if (user != null)
                {
                    user.FullName = dto.FullName;
                    user.PhoneNumber = dto.PhoneNumber;
                    user.UpdatedAt = DateTime.UtcNow;

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

                _unitOfWork.Repository<Teacher>().Update(teacher);
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
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì  ÕœÌÀ «·„⁄·„: {ex.Message}" });
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(id);
                if (teacher == null) return false;

                // ??? Soft Delete
                teacher.IsDeleted = true;
                teacher.DeletedAt = DateTime.UtcNow;
                teacher.DeletedBy = id.ToString();

                _unitOfWork.Repository<Teacher>().Update(teacher);
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
                throw new ValidationException(new List<string> { "·« Ì„ﬂ‰ Õ–› Â–« «·„⁄·„ ·√‰Â „— »ÿ »»Ì«‰«  √Œ—Ï." });
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"Œÿ√ ›Ì Õ–› «·„⁄·„: {ex.Message}" });
            }
        }

    }
}
