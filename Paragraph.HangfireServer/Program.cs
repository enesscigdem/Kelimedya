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

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ParagraphDbContext>(options =>
{
    options.UseSqlServer(connectionString, y => y.UseNetTopologySuite());
});

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<AppSettings>>()?.Value);

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<MailSettings>>()?.Value);

builder.Services.Configure<ImageConfig>(builder.Configuration.GetSection("ImageConfig"));
builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<ImageConfig>>()?.Value);

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
    options.Queues = new[] { QUEUENAMES.RECURRING, QUEUENAMES.SMS, QUEUENAMES.MAIL, QUEUENAMES.DEFAULT };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

app.Run();