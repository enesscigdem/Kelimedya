using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Paragraph.Core;
using Paragraph.Core.IdentityEntities;
using Paragraph.Core.Interfaces.Business;
using Paragraph.HangfireServer;
using Paragraph.HangfireServer.Services;
using Paragraph.Persistence;
using Paragraph.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ParagraphDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<AppSettings>>()?.Value);


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddIdentity<CustomUser, CustomRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddDefaultTokenProviders().AddEntityFrameworkStores<ParagraphDbContext>();

builder.Services.AddServicesRegistration();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add the processing server as IHostedService
builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { Queuenames.RECURRING, Queuenames.SMS, Queuenames.MAIL, Queuenames.DEFAULT };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.AttachMyHangfireJobs();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();