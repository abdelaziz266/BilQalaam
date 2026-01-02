using AutoMapper;
using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.DTOs.Students;
using BilQalaam.Application.DTOs.Supervisors;
using BilQalaam.Application.DTOs.Teachers;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Application.Results;
using BilQalaam.Domain.Entities;
using BilQalaam.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BilQalaam.Application.Services
{
    public class SupervisorService : ISupervisorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SupervisorService(
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

        public async Task<Result<PaginatedResponseDto<SupervisorResponseDto>>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchText = null)
        {
            try
            {
                var query = _unitOfWork.Repository<Supervisor>().Query();

                // Search by name or email
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var lowerSearchText = searchText.ToLower();
                    query = query.Where(s =>
                        s.SupervisorName.ToLower().Contains(lowerSearchText) );
                }

                var totalCount = await query.CountAsync();
                var supervisors = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Result<PaginatedResponseDto<SupervisorResponseDto>>.Success(new PaginatedResponseDto<SupervisorResponseDto>
                {
                    Items = _mapper.Map<IEnumerable<SupervisorResponseDto>>(supervisors),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    PagesCount = pagesCount
                });
            }
            catch (Exception ex)
            {
                return Result<PaginatedResponseDto<SupervisorResponseDto>>.Failure($"خطأ في جلب المشرفين: {ex.Message}");
            }
        }

        public async Task<SupervisorResponseDto?> GetByIdAsync(int id)
        {
            try
            {
                var supervisor = await _unitOfWork
                    .Repository<Supervisor>()
                    .GetByIdAsync(id);

                return supervisor == null ? null : _mapper.Map<SupervisorResponseDto>(supervisor);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب المشرف: {ex.Message}" });
            }
        }

        public async Task<Result<int>> CreateAsync(CreateSupervisorDto dto, string createdByUserId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // التحقق من عدم تكرار الايميل
                var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUserByEmail != null)
                    return Result<int>.Failure(new List<string> { "البريد الإلكتروني مستخدم بالفعل" });

                // التحقق من عدم تكرار اسم المشرف
                var supervisors = await _unitOfWork.Repository<Supervisor>().FindAsync(s => s.SupervisorName == dto.FullName);
                if (supervisors.Any())
                    return Result<int>.Failure(new List<string> { "اسم المشرف مستخدم بالفعل" });

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    FullName = dto.FullName,
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Result<int>.Failure(errors);
                }

                const string roleName = "Admin";
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new IdentityRole(roleName));

                await _userManager.AddToRoleAsync(user, roleName);

                var supervisor = new Supervisor
                {
                    SupervisorName = dto.FullName,
                    CountryCode = dto.CountryCode,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    HourlyRate = dto.HourlyRate,
                    Currency = dto.Currency,
                    StartDate = dto.StartDate,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                };

                await _unitOfWork.Repository<Supervisor>().AddAsync(supervisor);
                await _unitOfWork.CompleteAsync();

                await transaction.CommitAsync();

                return Result<int>.Success(supervisor.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<int>.Failure(new List<string> { $"خطأ في إنشاء المشرف: {ex.Message}" });
            }
        }

        public async Task<Result<bool>> UpdateAsync(int id, UpdateSupervisorDto dto, string updatedByUserId)
        {
            try
            {
                var supervisor = await _unitOfWork.Repository<Supervisor>().GetByIdAsync(id);
                if (supervisor == null)
                    return Result<bool>.Failure(new List<string> { "المشرف غير موجود" });

                // التحقق من عدم تكرار اسم المشرف (إذا تم تغييره)
                if (supervisor.SupervisorName != dto.FullName)
                {
                    var existingSupervisor = await _unitOfWork.Repository<Supervisor>()
                        .FindAsync(s => s.SupervisorName == dto.FullName && s.Id != id);
                    if (existingSupervisor.Any())
                        return Result<bool>.Failure(new List<string> { "اسم المشرف مستخدم بالفعل" });
                }

                supervisor.SupervisorName = dto.FullName;
                supervisor.CountryCode = dto.CountryCode;
                supervisor.PhoneNumber = dto.PhoneNumber;
                supervisor.HourlyRate = dto.HourlyRate;
                supervisor.Currency = dto.Currency;
                supervisor.StartDate = dto.StartDate;
                supervisor.UpdatedAt = DateTime.UtcNow;
                supervisor.UpdatedBy = updatedByUserId;

                var user = await _userManager.FindByIdAsync(supervisor.UserId);
                if (user != null)
                {
                    user.FullName = dto.FullName;
                    user.PhoneNumber = dto.PhoneNumber;
                    user.UpdatedAt = DateTime.UtcNow;

                    // تحديث كلمة المرور إذا تم توفيرها
                    if (!string.IsNullOrWhiteSpace(dto.Password))
                    {
                        var removeResult = await _userManager.RemovePasswordAsync(user);
                        if (!removeResult.Succeeded)
                        {
                            var errors = removeResult.Errors.Select(e => e.Description).ToList();
                            return Result<bool>.Failure(errors);
                        }

                        var addResult = await _userManager.AddPasswordAsync(user, dto.Password);
                        if (!addResult.Succeeded)
                        {
                            var errors = addResult.Errors.Select(e => e.Description).ToList();
                            return Result<bool>.Failure(errors);
                        }
                    }

                    await _userManager.UpdateAsync(user);
                }

                _unitOfWork.Repository<Supervisor>().Update(supervisor);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(new List<string> { $"خطأ في تحديث المشرف: {ex.Message}" });
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            try
            {
                var supervisor = await _unitOfWork.Repository<Supervisor>().GetByIdAsync(id);
                if (supervisor == null)
                    return Result<bool>.Failure("المشرف غير موجود");

                // التحقق من وجود معلمين مرتبطن بالمشرف
                var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.SupervisorId == id && !t.IsDeleted);
                if (teachers.Any())
                    return Result<bool>.Failure("لا يمكن حذف المشرف لأن هناك معلمين مرتبطين به");

                // التحقق من وجود عائلات مرتبطة بالمشرف
                var families = await _unitOfWork.Repository<Family>().FindAsync(f => f.SupervisorId == id && !f.IsDeleted);
                if (families.Any())
                    return Result<bool>.Failure("لا يمكن حذف المشرف لأن هناك عائلات مرتبطة به");

                var user = await _userManager.FindByIdAsync(supervisor.UserId);
                if (user != null)
                {
                    user.IsDeleted = true;
                    user.DeletedAt = DateTime.UtcNow;
                    user.DeletedBy = id.ToString();
                    await _userManager.UpdateAsync(user);
                }

                supervisor.IsDeleted = true;
                supervisor.DeletedAt = DateTime.UtcNow;
                supervisor.DeletedBy = id.ToString();

                _unitOfWork.Repository<Supervisor>().Update(supervisor);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطأ في حذف المشرف: {ex.Message}");
            }
        }

        public async Task<Result<SupervisorDetailsDto>> GetSupervisorDetailsAsync(int supervisorId)
        {
            try
            {
                // جيب بيانات المشرف
                var supervisor = await _unitOfWork.Repository<Supervisor>().GetByIdAsync(supervisorId);
                if (supervisor == null)
                    return Result<SupervisorDetailsDto>.Failure("المشرف غير موجود");

                // جيب المعلمين المرتبطين بالمشرف
                var teachers = await _unitOfWork.Repository<Teacher>()
                    .FindAsync(t => t.SupervisorId == supervisorId && !t.IsDeleted);

                // جيب العائلات المرتبطة بالمشرف
                var families = await _unitOfWork.Repository<Family>()
                    .FindAsync(f => f.SupervisorId == supervisorId && !f.IsDeleted);

                // جيب الطلاب المرتبطين بالمعلمين
                var teacherIds = teachers.Select(t => t.Id).ToList();
                var students = new List<Student>();
                if (teacherIds.Any())
                {
                    students = (await _unitOfWork.Repository<Student>()
                        .FindAsync(s => teacherIds.Contains(s.TeacherId) && !s.IsDeleted))
                        .ToList();
                }

                var supervisorDetailsDto = new SupervisorDetailsDto
                {
                    SupervisorId = supervisor.Id,
                    SupervisorName = supervisor.SupervisorName,
                    Teachers = _mapper.Map<List<TeacherResponseDto>>(teachers),
                    Families = _mapper.Map<List<FamilyResponseDto>>(families),
                    Students = _mapper.Map<List<StudentResponseDto>>(students),
                    TotalTeachers = teachers.Count(),
                    TotalFamilies = families.Count(),
                    TotalStudents = students.Count
                };

                return Result<SupervisorDetailsDto>.Success(supervisorDetailsDto);
            }
            catch (Exception ex)
            {
                return Result<SupervisorDetailsDto>.Failure($"خطأ في جلب تفاصيل المشرف: {ex.Message}");
            }
        }

        public async Task<Result<List<SupervisorDetailsDto>>> GetMultipleSupervisorsDetailsAsync(IEnumerable<int> supervisorIds)
        {
            try
            {
                var supervisorIdList = supervisorIds.ToList();
                if (!supervisorIdList.Any())
                    return Result<List<SupervisorDetailsDto>>.Failure("يجب تحديد معرف واحد على الأقل");

                var supervisorDetailsList = new List<SupervisorDetailsDto>();

                // جيب بيانات كل المشرفين
                var supervisors = await _unitOfWork.Repository<Supervisor>()
                    .FindAsync(s => supervisorIdList.Contains(s.Id) && !s.IsDeleted);

                if (!supervisors.Any())
                    return Result<List<SupervisorDetailsDto>>.Failure("لم يتم العثور على مشرفين");

                // جيب جميع المعلمين المرتبطين بأي من المشرفين
                var allTeachers = await _unitOfWork.Repository<Teacher>()
                    .FindAsync(t => supervisorIdList.Contains(t.SupervisorId.Value) && !t.IsDeleted);

                // جيب جميع العائلات المرتبطة بأي من المشرفين
                var allFamilies = await _unitOfWork.Repository<Family>()
                    .FindAsync(f => supervisorIdList.Contains(f.SupervisorId.Value) && !f.IsDeleted);

                // جيب جميع الطلاب المرتبطين بالمعلمين
                var teacherIds = allTeachers.Select(t => t.Id).ToList();
                var allStudents = new List<Student>();
                if (teacherIds.Any())
                {
                    allStudents = (await _unitOfWork.Repository<Student>()
                        .FindAsync(s => teacherIds.Contains(s.TeacherId) && !s.IsDeleted))
                        .ToList();
                }

                // بناء استجابة لكل مشرف
                foreach (var supervisor in supervisors)
                {
                    var supervisorTeachers = allTeachers.Where(t => t.SupervisorId == supervisor.Id).ToList();
                    var supervisorFamilies = allFamilies.Where(f => f.SupervisorId == supervisor.Id).ToList();
                    var supervisorTeacherIds = supervisorTeachers.Select(t => t.Id).ToList();
                    var supervisorStudents = allStudents.Where(s => supervisorTeacherIds.Contains(s.TeacherId)).ToList();

                    supervisorDetailsList.Add(new SupervisorDetailsDto
                    {
                        SupervisorId = supervisor.Id,
                        SupervisorName = supervisor.SupervisorName,
                        Teachers = _mapper.Map<List<TeacherResponseDto>>(supervisorTeachers),
                        Families = _mapper.Map<List<FamilyResponseDto>>(supervisorFamilies),
                        Students = _mapper.Map<List<StudentResponseDto>>(supervisorStudents),
                        TotalTeachers = supervisorTeachers.Count,
                        TotalFamilies = supervisorFamilies.Count(),
                        TotalStudents = supervisorStudents.Count
                    });
                }

                return Result<List<SupervisorDetailsDto>>.Success(supervisorDetailsList);
            }
            catch (Exception ex)
            {
                return Result<List<SupervisorDetailsDto>>.Failure($"خطأ في جلب تفاصيل المشرفين: {ex.Message}");
            }
        }
    }
}
