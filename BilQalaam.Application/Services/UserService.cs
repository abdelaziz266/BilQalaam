using AutoMapper;
using BilQalaam.Application.DTOs.Users;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BilQalaam.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<(IEnumerable<UserResponseDto>, int)> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                
                var totalCount = users.Count();
                var paginatedUsers = users
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return (_mapper.Map<IEnumerable<UserResponseDto>>(paginatedUsers), totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في جلب المستخدمين: {ex.Message}");
            }
        }

        public async Task<UserResponseDto?> GetByIdAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                return user == null ? null : _mapper.Map<UserResponseDto>(user);
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في جلب المستخدم: {ex.Message}");
            }
        }

        public async Task<string> CreateAsync(CreateUserDto dto)
        {
            try
            {
                // التحقق من عدم تكرار الايميل
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    throw new Exception("البريد الإلكتروني مستخدم بالفعل");

                var user = _mapper.Map<ApplicationUser>(dto);
                user.UserName = dto.Email;
                user.CreatedAt = DateTime.UtcNow;

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

                if (dto.Role.HasValue)
                {
                    var roleName = dto.Role.Value.ToString();

                    if (!await _roleManager.RoleExistsAsync(roleName))
                        await _roleManager.CreateAsync(new IdentityRole(roleName));

                    await _userManager.AddToRoleAsync(user, roleName);
                }

                await _unitOfWork.CompleteAsync();
                return user.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في إنشاء المستخدم: {ex.Message}");
            }
        }

        public async Task<bool> UpdateAsync(string id, UpdateUserDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return false;

                // التحقق من عدم تكرار الايميل (إذا تم تغييره)
                if (!string.IsNullOrEmpty(dto.Email) && user.Email != dto.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                    if (existingUser != null)
                        throw new Exception("البريد الإلكتروني مستخدم بالفعل");
                }

                _mapper.Map(dto, user);
                user.UpdatedAt = DateTime.UtcNow;

                // تحديث كلمة المرور إذا تم توفيرها
                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    var removeResult = await _userManager.RemovePasswordAsync(user);
                    if (!removeResult.Succeeded)
                    {
                        var errors = removeResult.Errors.Select(e => e.Description).ToList();
                        throw new Exception(string.Join(", ", errors));
                    }

                    var addResult = await _userManager.AddPasswordAsync(user, dto.Password);
                    if (!addResult.Succeeded)
                    {
                        var errors = addResult.Errors.Select(e => e.Description).ToList();
                        throw new Exception(string.Join(", ", errors));
                    }
                }

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في تحديث المستخدم: {ex.Message}");
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return false;

                // 🗑️ Soft Delete للـ ApplicationUser
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                user.DeletedBy = id;

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في حذف المستخدم: {ex.Message}");
            }
        }
    }
}
