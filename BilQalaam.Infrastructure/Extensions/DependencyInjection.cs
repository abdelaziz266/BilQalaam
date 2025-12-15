using AutoMapper;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Mapping;
using BilQalaam.Application.Services;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Infrastructure.Persistence;
using BilQalaam.Infrastructure.Repositories.Implementations;
using BilQalaam.Infrastructure.Repositories.Interfaces;
using BilQalaam.Infrastructure.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BilQalaam.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ✅ Database Context
            services.AddDbContext<BilQalaamDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // ✅ AutoMapper
            services.AddAutoMapper(typeof(UserProfile).Assembly);

            // ✅ Unit Of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ✅ Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // ✅ Application Services
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
