using AutoMapper;
using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.DTOs.Students;
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
            string userId,
            string? searchText = null)
        {
            try
            {
                IQueryable<Teacher> query = _unitOfWork
                    .Repository<Teacher>()
                    .Query()
                    .Include(t => t.Supervisor);

                // Search by name, email, or phone number
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var lowerSearchText = searchText.ToLower();
                    query = query.Where(t =>
                        t.TeacherName.ToLower().Contains(lowerSearchText));
                }

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
                    CountryCode = dto.CountryCode,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    SupervisorId = supervisorId,
                    HourlyRate = dto.HourlyRate,
                    Currency = dto.Currency,
                    StartDate = dto.StartDate,
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
                teacher.CountryCode = dto.CountryCode;
                teacher.PhoneNumber = dto.PhoneNumber;
                teacher.SupervisorId = dto.SupervisorId;
                teacher.HourlyRate = dto.HourlyRate;
                teacher.Currency = dto.Currency;
                teacher.StartDate = dto.StartDate;
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
                var studentTeachers = await _unitOfWork.Repository<StudentTeacher>().FindAsync(st => st.TeacherId == id);
                if (studentTeachers.Any())
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

        public async Task<Result<TeacherDetailsDto>> GetTeacherDetailsAsync(int teacherId)
        {
            try
            {
                // ÃÌ» »Ì«‰«  «·„⁄·„
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);
                if (teacher == null)
                    return Result<TeacherDetailsDto>.Failure("«·„⁄·„ €Ì— „ÊÃÊœ");

                // ÃÌ» «·⁄«∆·«  «·„— »ÿ… („‰ Œ·«· ‰›” Supervisor)
                var families = new List<Family>();
                if (teacher.SupervisorId.HasValue)
                {
                    families = (await _unitOfWork.Repository<Family>()
                        .FindAsync(f => f.SupervisorId == teacher.SupervisorId && !f.IsDeleted))
                        .ToList();
                }

                // ÃÌ» «·ÿ·«» «·„— »ÿÌ‰ »«·„⁄·„
                var studentTeachers = await _unitOfWork.Repository<StudentTeacher>()
                    .Query()
                    .Include(st => st.Student)
                    .Where(st => st.TeacherId == teacherId && !st.Student.IsDeleted)
                    .ToListAsync();
                var students = studentTeachers.Select(st => st.Student).ToList();

                var teacherDetailsDto = new TeacherDetailsDto
                {
                    TeacherId = teacher.Id,
                    TeacherName = teacher.TeacherName,
                    Families = _mapper.Map<List<FamilyResponseDto>>(families),
                    Students = _mapper.Map<List<StudentResponseDto>>(students),
                    TotalFamilies = families.Count,
                    TotalStudents = students.Count
                };

                return Result<TeacherDetailsDto>.Success(teacherDetailsDto);
            }
            catch (Exception ex)
            {
                return Result<TeacherDetailsDto>.Failure($"Œÿ√ ›Ì Ã·»  ›«’Ì· «·„⁄·„: {ex.Message}");
            }
        }

        public async Task<Result<List<TeacherDetailsDto>>> GetMultipleTeachersDetailsAsync(IEnumerable<int> teacherIds)
        {
            try
            {
                var teacherIdList = teacherIds.ToList();
                if (!teacherIdList.Any())
                    return Result<List<TeacherDetailsDto>>.Failure("ÌÃ»  ÕœÌœ „⁄—› Ê«Õœ ⁄·Ï «·√ﬁ·");

                var teacherDetailsList = new List<TeacherDetailsDto>();

                // ÃÌ» »Ì«‰«  ﬂ· «·„⁄·„Ì‰
                var teachers = await _unitOfWork.Repository<Teacher>()
                    .FindAsync(t => teacherIdList.Contains(t.Id) && !t.IsDeleted);

                if (!teachers.Any())
                    return Result<List<TeacherDetailsDto>>.Failure("·„ Ì „ «·⁄ÀÊ— ⁄·Ï „⁄·„Ì‰");

                // ÃÌ» Ã„Ì⁄ «·⁄«∆·«  «·„— »ÿ… »«·„⁄·„Ì‰ („‰ Œ·«· Supervisor)
                var supervisorIds = teachers.Where(t => t.SupervisorId.HasValue)
                    .Select(t => t.SupervisorId.Value)
                    .Distinct()
                    .ToList();

                var allFamilies = new List<Family>();
                if (supervisorIds.Any())
                {
                    allFamilies = (await _unitOfWork.Repository<Family>()
                        .FindAsync(f => supervisorIds.Contains(f.SupervisorId.Value) && !f.IsDeleted))
                        .ToList();
                }

                // ÃÌ» Ã„Ì⁄ «·ÿ·«» «·„— »ÿÌ‰ »«·„⁄·„Ì‰
                var allStudentTeachers = await _unitOfWork.Repository<StudentTeacher>()
                    .Query()
                    .Include(st => st.Student)
                    .Where(st => teacherIdList.Contains(st.TeacherId) && !st.Student.IsDeleted)
                    .ToListAsync();

                // »‰«¡ «” Ã«»… ·ﬂ· „⁄·„
                foreach (var teacher in teachers)
                {
                    var teacherFamilies = teacher.SupervisorId.HasValue
                        ? allFamilies.Where(f => f.SupervisorId == teacher.SupervisorId).ToList()
                        : new List<Family>();

                    var teacherStudents = allStudentTeachers
                        .Where(st => st.TeacherId == teacher.Id)
                        .Select(st => st.Student)
                        .ToList();

                    teacherDetailsList.Add(new TeacherDetailsDto
                    {
                        TeacherId = teacher.Id,
                        TeacherName = teacher.TeacherName,
                        Families = _mapper.Map<List<FamilyResponseDto>>(teacherFamilies),
                        Students = _mapper.Map<List<StudentResponseDto>>(teacherStudents),
                        TotalFamilies = teacherFamilies.Count,
                        TotalStudents = teacherStudents.Count
                    });
                }

                return Result<List<TeacherDetailsDto>>.Success(teacherDetailsList);
            }
            catch (Exception ex)
            {
                return Result<List<TeacherDetailsDto>>.Failure($"Œÿ√ ›Ì Ã·»  ›«’Ì· «·„⁄·„Ì‰: {ex.Message}");
            }
        }
    }
}
