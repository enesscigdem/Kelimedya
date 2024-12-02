using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Paragraph.Core.IdentityEntities;
using Paragraph.Core.Interfaces.Business;
using Paragraph.HangfireServer.Services;
using Paragraph.Persistence;
using Paragraph.Services.Implementations;
using Paragraph.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Identity services
builder.Services.AddIdentity<CustomUser, CustomRole>(options =>
{
    // Şifre kuralları
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Kilitleme kuralları
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Kullanıcı kuralları
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ParagraphDbContext>() // EF Core üzerinden Identity işlemleri
.AddDefaultTokenProviders();

// Add Application Services
builder.Services.AddScoped<IAuthService, AuthService>(); // IAuthService ve implementasyonu
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add DbContext
builder.Services.AddDbContext<ParagraphDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS middleware
app.UseCors("AllowAll");

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // API Controller'ları mapler

app.Run();
