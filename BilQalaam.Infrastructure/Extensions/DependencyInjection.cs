using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Mapping;
using BilQalaam.Application.Repositories.Interfaces;
using BilQalaam.Application.Services;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Infrastructure.Auth;
using BilQalaam.Infrastructure.Persistence;
using BilQalaam.Infrastructure.Repositories.Implementations;
using BilQalaam.Infrastructure.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BilQalaam.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILessonService, LessonService>();
            services.AddScoped<IFamilyService, FamilyService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<ISupervisorService, SupervisorService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
