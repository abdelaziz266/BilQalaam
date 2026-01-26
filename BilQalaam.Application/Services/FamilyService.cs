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
    public class FamilyService : IFamilyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public FamilyService(
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

        public async Task<Result<PaginatedResponseDto<FamilyResponseDto>>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string role,
            string userId,
            string? searchText = null)     
        {
            try
            {
                IQueryable<Family> query = _unitOfWork
                    .Repository<Family>()
                    .Query()
                    .Include(f => f.Supervisor);

                // Search by name, email, or phone number
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var lowerSearchText = searchText.ToLower();
                    query = query.Where(f =>
                        f.FamilyName.ToLower().Contains(lowerSearchText));
                }

                // Admin يشوف العائلات المرتبطة به فقط
                if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null)
                        return Result<PaginatedResponseDto<FamilyResponseDto>>.Success(EmptyPaginatedResponse(pageNumber, pageSize));

                    query = query.Where(f => f.SupervisorId == supervisor.Id);
                }
                if (role == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null || !teacher.SupervisorId.HasValue)
                        return Result<PaginatedResponseDto<FamilyResponseDto>>.Success(EmptyPaginatedResponse(pageNumber, pageSize));

                    query = query.Where(f => f.SupervisorId == teacher.SupervisorId);
                }
                var totalCount = await query.CountAsync();
                var families = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Result<PaginatedResponseDto<FamilyResponseDto>>.Success(new PaginatedResponseDto<FamilyResponseDto>
                {
                    Items = _mapper.Map<IEnumerable<FamilyResponseDto>>(families),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    PagesCount = pagesCount
                });
            }
            catch (Exception ex)
            {
                return Result<PaginatedResponseDto<FamilyResponseDto>>.Failure($"خطأ في جلب العائلات: {ex.Message}");
            }
        }

        public async Task<Result<FamilyResponseDto>> GetByIdAsync(int id, string role, string userId)
        {
            try
            {
                var family = await _unitOfWork
                    .Repository<Family>()
                    .Query()
                    .Include(f => f.Supervisor)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (family == null)
                    return Result<FamilyResponseDto>.Failure("العائلة غير موجودة");

                // Admin يشوف العائلات المرتبطة به فقط
                if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null || family.SupervisorId != supervisor.Id)
                        return Result<FamilyResponseDto>.Failure("العائلة غير موجودة");
                }

                if (role == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null || !teacher.SupervisorId.HasValue || family.SupervisorId != teacher.SupervisorId)
                        return Result<FamilyResponseDto>.Failure("العائلة غير موجودة");
                }

                return Result<FamilyResponseDto>.Success(_mapper.Map<FamilyResponseDto>(family));
            }
            catch (Exception ex)
            {
                return Result<FamilyResponseDto>.Failure($"خطأ في جلب العائلة: {ex.Message}");
            }
        }

        public async Task<Result<int>> CreateAsync(CreateFamilyDto dto, string role, string userId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // التحقق من عدم تكرار الايميل
                var existingUserByEmail = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);
                if (existingUserByEmail != null)
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure("البريد الإلكتروني مستخدم بالفعل");
                }

                // التحقق من عدم تكرار اسم العائلة
                var families = await _unitOfWork.Repository<Family>().FindAsync(f => f.FamilyName == dto.FullName);
                if (families.Any())
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure("اسم العائلة مستخدم بالفعل");
                }

                // تحديد SupervisorId حسب الـ Role
                int? supervisorId = dto.SupervisorId;
                if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null)
                    {
                        await transaction.RollbackAsync();
                        return Result<int>.Failure("لم يتم العثور على بيانات المشرف");
                    }
                    supervisorId = supervisor.Id;
                }

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    FullName = dto.FullName,
                    Role = UserRole.Family,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    await transaction.RollbackAsync();
                    return Result<int>.Failure(errors);
                }

                const string roleName = "Family";
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new IdentityRole(roleName));

                await _userManager.AddToRoleAsync(user, roleName);

                var family = new Family
                {
                    FamilyName = dto.FullName,
                    CountryCode = dto.CountryCode,
                    PhoneNumber = dto.PhoneNumber,
                    Country = dto.Country,
                    Email = dto.Email,
                    SupervisorId = supervisorId,
                    HourlyRate = dto.HourlyRate,
                    Currency = dto.Currency,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _unitOfWork.Repository<Family>().AddAsync(family);
                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();

                return Result<int>.Success(family.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<int>.Failure($"خطأ في إنشاء العائلة: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateAsync(int id, UpdateFamilyDto dto, string userId)
        {
            try
            {
                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(id);
                if (family == null)
                    return Result<bool>.Failure("العائلة غير موجودة");

                // التحقق من عدم تكرار اسم العائلة
                if (family.FamilyName != dto.FullName)
                {
                    var existingFamily = await _unitOfWork.Repository<Family>()
                        .FindAsync(f => f.FamilyName == dto.FullName && f.Id != id);
                    if (existingFamily.Any())
                        return Result<bool>.Failure("اسم العائلة مستخدم بالفعل");
                }

                family.FamilyName = dto.FullName;
                family.CountryCode = dto.CountryCode;
                family.PhoneNumber = dto.PhoneNumber;
                family.Country = dto.Country;
                family.SupervisorId = dto.SupervisorId;
                family.HourlyRate = dto.HourlyRate;
                family.Currency = dto.Currency;
                family.UpdatedAt = DateTime.UtcNow;
                family.UpdatedBy = userId;

                var user = await _userManager.FindByIdAsync(family.UserId);
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

                _unitOfWork.Repository<Family>().Update(family);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطأ في تحديث العائلة: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            try
            {
                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(id);
                if (family == null)
                    return Result<bool>.Failure("العائلة غير موجودة");

                // التحقق من وجود طلاب مرتبطين بالعائلة
                var students = await _unitOfWork.Repository<Student>().FindAsync(s => s.FamilyId == id && !s.IsDeleted);
                if (students.Any())
                    return Result<bool>.Failure("لا يمكن حذف العائلة لأن هناك طلاب مرتبطين بها");

                family.IsDeleted = true;
                family.DeletedAt = DateTime.UtcNow;
                family.DeletedBy = id.ToString();

                var familyUser = await _userManager.FindByIdAsync(family.UserId);
                if (familyUser != null)
                {
                    familyUser.IsDeleted = true;
                    familyUser.DeletedAt = DateTime.UtcNow;
                    familyUser.DeletedBy = id.ToString();
                    await _userManager.UpdateAsync(familyUser);
                }

                _unitOfWork.Repository<Family>().Update(family);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطأ في حذف العائلة: {ex.Message}");
            }
        }

        /// <summary>
        /// Get families by teacher ID (same supervisor)
        /// </summary>
        public async Task<Result<PaginatedResponseDto<FamilyResponseDto>>> GetByTeacherIdAsync(
            int teacherId,
            int pageNumber,
            int pageSize)
        {
            try
            {
                // جيب الـ Teacher
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);
                if (teacher == null)
                    return Result<PaginatedResponseDto<FamilyResponseDto>>.Failure("المعلم غير موجود");

                // لو مفيش Supervisor مربوط
                if (!teacher.SupervisorId.HasValue)
                    return Result<PaginatedResponseDto<FamilyResponseDto>>.Success(EmptyPaginatedResponse(pageNumber, pageSize));

                // جيب كل الـ Families المرتبطة بنفس الـ Supervisor
                var query = _unitOfWork
                    .Repository<Family>()
                    .Query()
                    .Include(f => f.Supervisor)
                    .Where(f => f.SupervisorId == teacher.SupervisorId);

                var totalCount = await query.CountAsync();
                var families = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Result<PaginatedResponseDto<FamilyResponseDto>>.Success(new PaginatedResponseDto<FamilyResponseDto>
                {
                    Items = _mapper.Map<IEnumerable<FamilyResponseDto>>(families),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    PagesCount = pagesCount
                });
            }
            catch (Exception ex)
            {
                return Result<PaginatedResponseDto<FamilyResponseDto>>.Failure($"خطأ في جلب العائلات: {ex.Message}");
            }
        }

        #region Private Helpers

        private async Task<Supervisor?> GetSupervisorByUserId(string userId)
        {
            var supervisors = await _unitOfWork.Repository<Supervisor>().FindAsync(s => s.UserId == userId);
            return supervisors.FirstOrDefault();
        }

        private async Task<Teacher?> GetTeacherByUserId(string userId)
        {
            var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.UserId == userId);
            return teachers.FirstOrDefault();
        }

        private static PaginatedResponseDto<FamilyResponseDto> EmptyPaginatedResponse(int pageNumber, int pageSize)
        {
            return new PaginatedResponseDto<FamilyResponseDto>
            {
                Items = new List<FamilyResponseDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0,
                PagesCount = 0
            };
        }

        #endregion

        public async Task<Result<FamilyDetailsDto>> GetFamilyDetailsAsync(int familyId)
        {
            try
            {
                // جيب بيانات العائلة
                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(familyId);
                if (family == null)
                    return Result<FamilyDetailsDto>.Failure("العائلة غير موجودة");

                // جيب المعلمين المرتبطين (من خلال نفس Supervisor)
                var teachers = new List<Teacher>();
                if (family.SupervisorId.HasValue)
                {
                    teachers = (await _unitOfWork.Repository<Teacher>()
                        .FindAsync(t => t.SupervisorId == family.SupervisorId && !t.IsDeleted))
                        .ToList();
                }

                // جيب الطلاب المرتبطين بالعائلة
                var students = (await _unitOfWork.Repository<Student>()
                    .FindAsync(s => s.FamilyId == familyId && !s.IsDeleted))
                    .ToList();

                var familyDetailsDto = new FamilyDetailsDto
                {
                    FamilyId = family.Id,
                    FamilyName = family.FamilyName,
                    Teachers = _mapper.Map<List<TeacherResponseDto>>(teachers),
                    Students = _mapper.Map<List<StudentResponseDto>>(students),
                    TotalTeachers = teachers.Count,
                    TotalStudents = students.Count
                };

                return Result<FamilyDetailsDto>.Success(familyDetailsDto);
            }
            catch (Exception ex)
            {
                return Result<FamilyDetailsDto>.Failure($"خطأ في جلب تفاصيل العائلة: {ex.Message}");
            }
        }

        public async Task<Result<List<FamilyDetailsDto>>> GetMultipleFamiliesDetailsAsync(IEnumerable<int> familyIds)
        {
            try
            {
                var familyIdList = familyIds.ToList();
                if (!familyIdList.Any())
                    return Result<List<FamilyDetailsDto>>.Failure("يجب تحديد معرف واحد على الأقل");

                var familyDetailsList = new List<FamilyDetailsDto>();

                // جيب بيانات كل العائلات
                var families = await _unitOfWork.Repository<Family>()
                    .FindAsync(f => familyIdList.Contains(f.Id) && !f.IsDeleted);

                if (!families.Any())
                    return Result<List<FamilyDetailsDto>>.Failure("لم يتم العثور على عائلات");

                // جيب جميع المعلمين المرتبطين بالعائلات (من خلال Supervisor)
                var supervisorIds = families.Where(f => f.SupervisorId.HasValue)
                    .Select(f => f.SupervisorId.Value)
                    .Distinct()
                    .ToList();

                var allTeachers = new List<Teacher>();
                if (supervisorIds.Any())
                {
                    allTeachers = (await _unitOfWork.Repository<Teacher>()
                        .FindAsync(t => supervisorIds.Contains(t.SupervisorId.Value) && !t.IsDeleted))
                        .ToList();
                }

                // جيب جميع الطلاب المرتبطة بالعائلات
                var allStudents = (await _unitOfWork.Repository<Student>()
                    .FindAsync(s => familyIdList.Contains(s.FamilyId.Value) && !s.IsDeleted))
                    .ToList();

                // بناء استجابة لكل عائلة
                foreach (var family in families)
                {
                    var familyTeachers = family.SupervisorId.HasValue
                        ? allTeachers.Where(t => t.SupervisorId == family.SupervisorId).ToList()
                        : new List<Teacher>();

                    var familyStudents = allStudents.Where(s => s.FamilyId == family.Id).ToList();

                    familyDetailsList.Add(new FamilyDetailsDto
                    {
                        FamilyId = family.Id,
                        FamilyName = family.FamilyName,
                        Teachers = _mapper.Map<List<TeacherResponseDto>>(familyTeachers),
                        Students = _mapper.Map<List<StudentResponseDto>>(familyStudents),
                        TotalTeachers = familyTeachers.Count,
                        TotalStudents = familyStudents.Count
                    });
                }

                return Result<List<FamilyDetailsDto>>.Success(familyDetailsList);
            }
            catch (Exception ex)
            {
                return Result<List<FamilyDetailsDto>>.Failure($"خطأ في جلب تفاصيل العائلات: {ex.Message}");
            }
        }
    }
}
