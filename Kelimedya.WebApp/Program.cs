using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Kelimedya.Core;
using Kelimedya.Core.Interfaces.Business;
using Kelimedya.HangfireServer.Services;
using Kelimedya.Persistence;
using System.Text;
using System.Text.Json.Serialization;
using Kelimedya.Services.Implementations;
using Kelimedya.Services.Interfaces;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});;

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<AppSettings>>()?.Value);

builder.Services.AddHttpClient("DefaultApi", (provider, client) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["AppSettings:ApiUrl"]);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbPassword = builder.Configuration["ConnectionStrings:DefaultConnection:Password"];
connectionString += $"Password={dbPassword}";
builder.Services.AddDbContext<KelimedyaDbContext>(options =>
    options.UseSqlServer(connectionString));

// WebApp/Program.cs içinde, builder oluşturulduktan sonra:
Console.WriteLine($"[WebApp Config] JWT Secret   : '{builder.Configuration["JWT:Secret"]}'");
Console.WriteLine($"[WebApp Config] JWT Issuer   : '{builder.Configuration["JWT:Issuer"]}'");
Console.WriteLine($"[WebApp Config] JWT Audience : '{builder.Configuration["JWT:Audience"]}'");


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("JWT");
        var key = Encoding.UTF8.GetBytes(jwt["Secret"]);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = ClaimTypes.Role,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var has = ctx.Request.Cookies.TryGetValue("AuthToken", out var token);
                Console.WriteLine($"[WebApp JWT] Cookie var mı? {has}. Token başı: '{(has ? token[..20] : "")}…'");
                if (has) ctx.Token = token;
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"[WebApp JWT] AuthFailed: {ctx.Exception.GetType().Name} — {ctx.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                var roles = string.Join(",", ctx.Principal.Claims
                    .Where(c => c.Type == "role")
                    .Select(c => c.Value));
                Console.WriteLine($"[WebApp JWT] TokenValidated. Roller: {roles}");
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization();


builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IWidgetService, WidgetService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
