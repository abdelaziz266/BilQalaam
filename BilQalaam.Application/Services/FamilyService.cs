using AutoMapper;
using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using BilQalaam.Domain.Enums;
using Microsoft.AspNetCore.Identity;

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

        public async Task<IEnumerable<FamilyResponseDto>> GetAllAsync()
        {
            var families = await _unitOfWork
                .Repository<Family>()
                .GetAllAsync();

            return _mapper.Map<IEnumerable<FamilyResponseDto>>(families);
        }

        public async Task<FamilyResponseDto?> GetByIdAsync(int id)
        {
            var family = await _unitOfWork
                .Repository<Family>()
                .GetByIdAsync(id);

            return family == null ? null : _mapper.Map<FamilyResponseDto>(family);
        }

        public async Task<int> CreateAsync(CreateFamilyDto dto, string createdByUserId)
        {
            // استخدام Transaction لضمان إما نجاح كل العمليات أو فشلها جميعاً
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // 1️⃣ إنشاء اليوزر أولاً
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
                    throw new ValidationException(errors);
                }

                // إضافة Role
                const string roleName = "Family";
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new IdentityRole(roleName));

                await _userManager.AddToRoleAsync(user, roleName);

                // 2️⃣ إنشاء العائلة مرتبطة باليوزر
                var family = new Family
                {
                    FamilyName = dto.FamilyName,
                    PhoneNumber = dto.PhoneNumber,
                    Country = dto.Country,
                    Email = dto.Email,
                    SupervisorId = dto.SupervisorId,
                    HourlyRate = dto.HourlyRate,
                    Currency = dto.Currency,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId // ⬅️ الـ User اللي عامل login
                };

                await _unitOfWork.Repository<Family>().AddAsync(family);
                await _unitOfWork.CompleteAsync();

                // ✅ تأكيد الـ Transaction
                await transaction.CommitAsync();

                return family.Id;
            }
            catch (Exception)
            {
                // ❌ التراجع عن كل العمليات في حالة حدوث خطأ
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int id, UpdateFamilyDto dto, string updatedByUserId)
        {
            var family = await _unitOfWork.Repository<Family>().GetByIdAsync(id);
            if (family == null) return false;

            // تحديث بيانات العائلة
            family.FamilyName = dto.FamilyName;
            family.PhoneNumber = dto.PhoneNumber;
            family.Country = dto.Country;
            family.SupervisorId = dto.SupervisorId;
            family.HourlyRate = dto.HourlyRate;
            family.Currency = dto.Currency;
            family.UpdatedAt = DateTime.UtcNow;
            family.UpdatedBy = updatedByUserId; // ⬅️ الـ User اللي عامل login

            // تحديث بيانات اليوزر
            var user = await _userManager.FindByIdAsync(family.UserId);
            if (user != null)
            {
                user.FullName = dto.FullName;
                user.PhoneNumber = dto.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            _unitOfWork.Repository<Family>().Update(family);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var family = await _unitOfWork.Repository<Family>().GetByIdAsync(id);
            if (family == null) return false;

            // حذف اليوزر
            var user = await _userManager.FindByIdAsync(family.UserId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            _unitOfWork.Repository<Family>().Delete(family);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
