using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Paragraph.Core;
using Paragraph.Core.Interfaces.Business;
using Paragraph.HangfireServer.Services;
using Paragraph.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<AppSettings>>()?.Value);
builder.Services.AddHttpClient("DefaultApi", (provider, client) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["AppSettings:ApiUrl"]);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbPassword = builder.Configuration["ConnectionStrings:DefaultConnection:Password"];
connectionString += $"Password={dbPassword}";

builder.Services.AddDbContext<ParagraphDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();