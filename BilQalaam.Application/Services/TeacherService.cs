using AutoMapper;
using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Teachers;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Results;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using BilQalaam.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Result<PaginatedResponseDto<TeacherResponseDto>>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string role,
            string userId)
        {
            try
            {
                IQueryable<Teacher> query = _unitOfWork
                    .Repository<Teacher>()
                    .Query()
                    .Include(t => t.Supervisor);

                // Admin Ì‘Ê› «·„⁄·„Ì‰ «·„— »ÿÌ‰ »Â ›ﬁÿ
                if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null)
                        return Result<PaginatedResponseDto<TeacherResponseDto>>.Success(EmptyPaginatedResponse(pageNumber, pageSize));

                    query = query.Where(t => t.SupervisorId == supervisor.Id);
                }

                var totalCount = await query.CountAsync();
                var teachers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Result<PaginatedResponseDto<TeacherResponseDto>>.Success(new PaginatedResponseDto<TeacherResponseDto>
                {
                    Items = _mapper.Map<IEnumerable<TeacherResponseDto>>(teachers),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    PagesCount = pagesCount
                });
            }
            catch (Exception ex)
            {
                return Result<PaginatedResponseDto<TeacherResponseDto>>.Failure($"Œÿ√ ›Ì Ã·» «·„⁄·„Ì‰: {ex.Message}");
            }
        }

        public async Task<Result<TeacherResponseDto>> GetByIdAsync(int id, string role, string userId)
        {
            try
            {
                var teacher = await _unitOfWork
                    .Repository<Teacher>()
                    .Query()
                    .Include(t => t.Supervisor)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (teacher == null)
                    return Result<TeacherResponseDto>.Failure("«·„⁄·„ €Ì— „ÊÃÊœ");

                // Admin Ì‘Ê› «·„⁄·„Ì‰ «·„— »ÿÌ‰ »Â ›ﬁÿ
                if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null || teacher.SupervisorId != supervisor.Id)
                        return Result<TeacherResponseDto>.Failure("«·„⁄·„ €Ì— „ÊÃÊœ");
                }

                return Result<TeacherResponseDto>.Success(_mapper.Map<TeacherResponseDto>(teacher));
            }
            catch (Exception ex)
            {
                return Result<TeacherResponseDto>.Failure($"Œÿ√ ›Ì Ã·» «·„⁄·„: {ex.Message}");
            }
        }

        public async Task<Result<int>> CreateAsync(CreateTeacherDto dto, string role, string userId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // «· Õﬁﬁ „‰ ⁄œ„  ﬂ—«— «·«Ì„Ì·
                var existingUserByEmail = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);
                if (existingUserByEmail != null)
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure("«·»—Ìœ «·≈·ﬂ —Ê‰Ì „” Œœ„ »«·›⁄·");
                }

                // «· Õﬁﬁ „‰ ⁄œ„  ﬂ—«— «”„ «·„⁄·„
                var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.TeacherName == dto.FullName);
                if (teachers.Any())
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure("«”„ «·„⁄·„ „” Œœ„ »«·›⁄·");
                }

                //  ÕœÌœ SupervisorId Õ”» «·‹ Role
                int? supervisorId = dto.SupervisorId;
                if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null)
                    {
                        await transaction.RollbackAsync();
                        return Result<int>.Failure("·„ Ì „ «·⁄ÀÊ— ⁄·Ï »Ì«‰«  «·„‘—›");
                    }
                    supervisorId = supervisor.Id;
                }

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
                    await transaction.RollbackAsync();
                    return Result<int>.Failure(errors);
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
                    SupervisorId = supervisorId,
                    HourlyRate = dto.HourlyRate,
                    Currency = dto.Currency,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _unitOfWork.Repository<Teacher>().AddAsync(teacher);
                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();

                return Result<int>.Success(teacher.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<int>.Failure($"Œÿ√ ›Ì ≈‰‘«¡ «·„⁄·„: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateAsync(int id, UpdateTeacherDto dto, string userId)
        {
            try
            {
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(id);
                if (teacher == null)
                    return Result<bool>.Failure("«·„⁄·„ €Ì— „ÊÃÊœ");

                // «· Õﬁﬁ „‰ ⁄œ„  ﬂ—«— «”„ «·„⁄·„
                if (teacher.TeacherName != dto.FullName)
                {
                    var existingTeacher = await _unitOfWork.Repository<Teacher>()
                        .FindAsync(t => t.TeacherName == dto.FullName && t.Id != id);
                    if (existingTeacher.Any())
                        return Result<bool>.Failure("«”„ «·„⁄·„ „” Œœ„ »«·›⁄·");
                }

                teacher.TeacherName = dto.FullName;
                teacher.PhoneNumber = dto.PhoneNumber;
                teacher.SupervisorId = dto.SupervisorId;
                teacher.HourlyRate = dto.HourlyRate;
                teacher.Currency = dto.Currency;
                teacher.UpdatedAt = DateTime.UtcNow;
                teacher.UpdatedBy = userId;

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
                            return Result<bool>.Failure(removeResult.Errors.Select(e => e.Description).ToList());

                        var addResult = await _userManager.AddPasswordAsync(user, dto.Password);
                        if (!addResult.Succeeded)
                            return Result<bool>.Failure(addResult.Errors.Select(e => e.Description).ToList());
                    }

                    await _userManager.UpdateAsync(user);
                }

                _unitOfWork.Repository<Teacher>().Update(teacher);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Œÿ√ ›Ì  ÕœÌÀ «·„⁄·„: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            try
            {
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(id);
                if (teacher == null)
                    return Result<bool>.Failure("«·„⁄·„ €Ì— „ÊÃÊœ");

                // «· Õﬁﬁ „‰ ÊÃÊœ ÿ·«» „— »ÿÌ‰ »«·„⁄·„
                var students = await _unitOfWork.Repository<Student>().FindAsync(s => s.TeacherId == id && !s.IsDeleted);
                if (students.Any())
                    return Result<bool>.Failure("·« Ì„ﬂ‰ Õ–› «·„⁄·„ ·√‰ Â‰«ﬂ ÿ·«» „— »ÿÌ‰ »Â");

                // «· Õﬁﬁ „‰ ÊÃÊœ œ—Ê” „— »ÿ… »«·„⁄·„
                var lessons = await _unitOfWork.Repository<Lesson>().FindAsync(l => l.TeacherId == id && !l.IsDeleted);
                if (lessons.Any())
                    return Result<bool>.Failure("·« Ì„ﬂ‰ Õ–› «·„⁄·„ ·√‰ Â‰«ﬂ œ—Ê” „— »ÿ… »Â");

                var teacherUser = await _userManager.FindByIdAsync(teacher.UserId);
                if (teacherUser != null)
                {
                    teacherUser.IsDeleted = true;
                    teacherUser.DeletedAt = DateTime.UtcNow;
                    teacherUser.DeletedBy = id.ToString();
                    await _userManager.UpdateAsync(teacherUser);
                }

                teacher.IsDeleted = true;
                teacher.DeletedAt = DateTime.UtcNow;
                teacher.DeletedBy = id.ToString();

                _unitOfWork.Repository<Teacher>().Update(teacher);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Œÿ√ ›Ì Õ–› «·„⁄·„: {ex.Message}");
            }
        }

        /// <summary>
        /// Get teachers by family ID (same supervisor)
        /// </summary>
        public async Task<Result<PaginatedResponseDto<TeacherResponseDto>>> GetByFamilyIdAsync(
            int familyId,
            int pageNumber,
            int pageSize)
        {
            try
            {
                // ÃÌ» «·‹ Family
                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(familyId);
                if (family == null)
                    return Result<PaginatedResponseDto<TeacherResponseDto>>.Failure("«·⁄«∆·… €Ì— „ÊÃÊœ…");

                // ·Ê „›Ì‘ Supervisor „—»Êÿ
                if (!family.SupervisorId.HasValue)
                    return Result<PaginatedResponseDto<TeacherResponseDto>>.Success(EmptyPaginatedResponse(pageNumber, pageSize));

                // ÃÌ» ﬂ· «·‹ Teachers «·„— »ÿÌ‰ »‰›” «·‹ Supervisor
                var query = _unitOfWork
                    .Repository<Teacher>()
                    .Query()
                    .Include(t => t.Supervisor)
                    .Where(t => t.SupervisorId == family.SupervisorId);

                var totalCount = await query.CountAsync();
                var teachers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Result<PaginatedResponseDto<TeacherResponseDto>>.Success(new PaginatedResponseDto<TeacherResponseDto>
                {
                    Items = _mapper.Map<IEnumerable<TeacherResponseDto>>(teachers),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    PagesCount = pagesCount
                });
            }
            catch (Exception ex)
            {
                return Result<PaginatedResponseDto<TeacherResponseDto>>.Failure($"Œÿ√ ›Ì Ã·» «·„⁄·„Ì‰: {ex.Message}");
            }
        }

        #region Private Helpers

        private async Task<Supervisor?> GetSupervisorByUserId(string userId)
        {
            var supervisors = await _unitOfWork.Repository<Supervisor>().FindAsync(s => s.UserId == userId);
            return supervisors.FirstOrDefault();
        }

        private static PaginatedResponseDto<TeacherResponseDto> EmptyPaginatedResponse(int pageNumber, int pageSize)
        {
            return new PaginatedResponseDto<TeacherResponseDto>
            {
                Items = new List<TeacherResponseDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0,
                PagesCount = 0
            };
        }

        #endregion
    }
}
