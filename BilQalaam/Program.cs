using BilQalaam.Api;
using BilQalaam.Application.Mapping;
using BilQalaam.Domain.Entities;
using BilQalaam.Infrastructure.Extensions;
using BilQalaam.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// =====================
// Infrastructure
// =====================
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

builder.Services.AddDbContext<BilQalaamDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// =====================
// Identity
// =====================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<BilQalaamDbContext>()
.AddDefaultTokenProviders();

// =====================
// JWT Authentication
// =====================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        ),
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
});

// =====================
// Controllers
// =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// =====================
// CORS 🔥 (حل OPTIONS 405)
// =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// =====================
// Swagger + JWT 🔒
// =====================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BilQalaam.Api",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();
// =====================
// Auto Apply Migrations + Seed
// =====================
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<BilQalaamDbContext>();
//    dbContext.Database.Migrate();

//    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//    const string superAdminEmail = "superadmin@bilqalaam.com";
//    const string superAdminPassword = "Aa@12345#";
//    const string superAdminRole = "SuperAdmin";

//    if (!await roleManager.RoleExistsAsync(superAdminRole))
//    {
//        await roleManager.CreateAsync(new IdentityRole(superAdminRole));
//    }

//    var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
//    if (superAdminUser == null)
//    {
//        superAdminUser = new ApplicationUser
//        {
//            UserName = superAdminEmail,
//            Email = superAdminEmail,
//            FullName = "Super Admin",
//            Role = BilQalaam.Domain.Enums.UserRole.SuperAdmin,
//            EmailConfirmed = true,
//            PhoneNumber = "0000000000"
//        };

//        var result = await userManager.CreateAsync(superAdminUser, superAdminPassword);
//        if (result.Succeeded)
//        {
//            await userManager.AddToRoleAsync(superAdminUser, superAdminRole);
//        }
//    }
//}



// =====================
// Middleware
// =====================


    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();


// 🔥 Exception Handler Middleware (قبل CORS)
app.UseMiddleware<ExceptionHandlerMiddleware>();

// 🔥 CORS لازم قبل Auth
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();