using BilQalaam.Application.Mapping;
using BilQalaam.Infrastructure.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Railway PORT
builder.WebHost.UseUrls($"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");

// =====================
// Services
// =====================
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

// ❌ مؤقتًا: لا DB – لا Identity – لا JWT
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// =====================
// CORS
// =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
    );
});

// =====================
// Swagger
// =====================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BilQalaam.Api",
        Version = "v1"
    });
});

var app = builder.Build();

// =====================
// Middleware
// =====================
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();
