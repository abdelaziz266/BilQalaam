using AutoMapper;
using BilQalaam.Application.DTOs.Supervisors;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using BilQalaam.Domain.Enums;
using Microsoft.AspNetCore.Identity;

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

        public async Task<IEnumerable<SupervisorResponseDto>> GetAllAsync()
        {
            var supervisors = await _unitOfWork
                .Repository<Supervisor>()
                .GetAllAsync();

            return _mapper.Map<IEnumerable<SupervisorResponseDto>>(supervisors);
        }

        public async Task<SupervisorResponseDto?> GetByIdAsync(int id)
        {
            var supervisor = await _unitOfWork
                .Repository<Supervisor>()
                .GetByIdAsync(id);

            return supervisor == null ? null : _mapper.Map<SupervisorResponseDto>(supervisor);
        }

        public async Task<int> CreateAsync(CreateSupervisorDto dto, string createdByUserId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1?? ≈‰‘«¡ «·ÌÊ“— √Ê·«
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    FullName = dto.FullName,
                    Role = UserRole.Admin, // ?? «·„‘—› ÌﬂÊ‰ Admin
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    throw new ValidationException(errors);
                }

                // ≈÷«›… Role
                const string roleName = "Admin";
                if (!await _roleManager.RoleExistsAsync(roleName))
                    await _roleManager.CreateAsync(new IdentityRole(roleName));

                await _userManager.AddToRoleAsync(user, roleName);

                // 2?? ≈‰‘«¡ «·„‘—› „— »ÿ »«·ÌÊ“—
                var supervisor = new Supervisor
                {
                    SupervisorName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    HourlyRate = dto.HourlyRate,
                    Currency = dto.Currency,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                };

                await _unitOfWork.Repository<Supervisor>().AddAsync(supervisor);
                await _unitOfWork.CompleteAsync();

                await transaction.CommitAsync();

                return supervisor.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int id, UpdateSupervisorDto dto, string updatedByUserId)
        {
            var supervisor = await _unitOfWork.Repository<Supervisor>().GetByIdAsync(id);
            if (supervisor == null) return false;

            //  ÕœÌÀ »Ì«‰«  «·„‘—›
            supervisor.SupervisorName = dto.SupervisorName;
            supervisor.PhoneNumber = dto.PhoneNumber;
            supervisor.HourlyRate = dto.HourlyRate;
            supervisor.Currency = dto.Currency;
            supervisor.UpdatedAt = DateTime.UtcNow;
            supervisor.UpdatedBy = updatedByUserId;

            //  ÕœÌÀ »Ì«‰«  «·ÌÊ“—
            var user = await _userManager.FindByIdAsync(supervisor.UserId);
            if (user != null)
            {
                user.FullName = dto.FullName;
                user.PhoneNumber = dto.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            _unitOfWork.Repository<Supervisor>().Update(supervisor);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var supervisor = await _unitOfWork.Repository<Supervisor>().GetByIdAsync(id);
            if (supervisor == null) return false;

            // Õ–› «·ÌÊ“—
            var user = await _userManager.FindByIdAsync(supervisor.UserId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            _unitOfWork.Repository<Supervisor>().Delete(supervisor);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
