using AutoMapper;
using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.Exceptions;
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

        public async Task<(IEnumerable<FamilyResponseDto>, int)> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var families = await _unitOfWork
                    .Repository<Family>()
                    .GetAllAsync();

                // 🔄 تحميل Supervisor للعائلات
                foreach (var family in families)
                {
                    if (family.SupervisorId.HasValue)
                    {
                        var supervisor = await _unitOfWork.Repository<Supervisor>().GetByIdAsync(family.SupervisorId.Value);
                        family.Supervisor = supervisor;
                    }
                }

                var totalCount = families.Count();
                var paginatedFamilies = families
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return (_mapper.Map<IEnumerable<FamilyResponseDto>>(paginatedFamilies), totalCount);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب العائلات: {ex.Message}" });
            }
        }

        public async Task<FamilyResponseDto?> GetByIdAsync(int id)
        {
            try
            {
                var family = await _unitOfWork
                    .Repository<Family>()
                    .GetByIdAsync(id);

                if (family != null && family.SupervisorId.HasValue)
                {
                    var supervisor = await _unitOfWork.Repository<Supervisor>().GetByIdAsync(family.SupervisorId.Value);
                    family.Supervisor = supervisor;
                }

                return family == null ? null : _mapper.Map<FamilyResponseDto>(family);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب العائلة: {ex.Message}" });
            }
        }

        public async Task<(IEnumerable<FamilyResponseDto>, int)> GetBySupervisorIdsAsync(
    IEnumerable<int> supervisorIds,
    int pageNumber,
    int pageSize)
        {
            try
            {
                var query = _unitOfWork
                    .Repository<Family>()
                    .Query()
                    .Include(f => f.Supervisor)
                    .Where(f => f.SupervisorId.HasValue &&
                                supervisorIds.Contains(f.SupervisorId.Value));

                var totalCount = await query.CountAsync();

                var families = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (_mapper.Map<IEnumerable<FamilyResponseDto>>(families), totalCount);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب عائلات المشرفين: {ex.Message}" });
            }
        }

        public async Task<Result<int>> CreateAsync(CreateFamilyDto dto, string createdByUserId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // التحقق من عدم تكرار الايميل
                var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUserByEmail != null)
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure(new List<string> { "البريد الإلكتروني مستخدم بالفعل" });
                }

                // التحقق من عدم تكرار اسم العائلة
                var families = await _unitOfWork.Repository<Family>().FindAsync(f => f.FamilyName == dto.FullName);
                if (families.Any())
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure(new List<string> { "اسم العائلة مستخدم بالفعل" });
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
                    PhoneNumber = dto.PhoneNumber,
                    Country = dto.Country,
                    Email = dto.Email,
                    SupervisorId = dto.SupervisorId,
                    HourlyRate = dto.HourlyRate,
                    Currency = dto.Currency,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                };

                await _unitOfWork.Repository<Family>().AddAsync(family);
                await _unitOfWork.CompleteAsync();

                await transaction.CommitAsync();

                return Result<int>.Success(family.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<int>.Failure(new List<string> { $"خطأ في إنشاء العائلة: {ex.Message}" });
            }
        }

        public async Task<Result<bool>> UpdateAsync(int id, UpdateFamilyDto dto, string updatedByUserId)
        {
            try
            {
                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(id);
                if (family == null)
                    return Result<bool>.Failure(new List<string> { "العائلة غير موجودة" });

                // التحقق من عدم تكرار اسم العائلة (إذا تم تغييره)
                if (family.FamilyName != dto.FullName)
                {
                    var existingFamily = await _unitOfWork.Repository<Family>()
                        .FindAsync(f => f.FamilyName == dto.FullName && f.Id != id);
                    if (existingFamily.Any())
                        return Result<bool>.Failure(new List<string> { "اسم العائلة مستخدم بالفعل" });
                }

                family.FamilyName = dto.FullName;
                family.PhoneNumber = dto.PhoneNumber;
                family.Country = dto.Country;
                family.SupervisorId = dto.SupervisorId;
                family.HourlyRate = dto.HourlyRate;
                family.Currency = dto.Currency;
                family.UpdatedAt = DateTime.UtcNow;
                family.UpdatedBy = updatedByUserId;

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

                _unitOfWork.Repository<Family>().Update(family);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(new List<string> { $"خطأ في تحديث العائلة: {ex.Message}" });
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            try
            {
                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(id);
                if (family == null)
                    return Result<bool>.Failure(new List<string> { "العائلة غير موجودة" });

                // 🗑️ Soft Delete
                family.IsDeleted = true;
                family.DeletedAt = DateTime.UtcNow;
                family.DeletedBy = id.ToString();

                _unitOfWork.Repository<Family>().Update(family);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(new List<string> { $"خطأ في حذف العائلة: {ex.Message}" });
            }
        }
    }
}
