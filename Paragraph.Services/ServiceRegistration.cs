using System.ComponentModel.Design;
using System.Resources;
using AutoMapper.EquivalencyExpression;
using Microsoft.Extensions.DependencyInjection;
using Paragraph.Core.Interfaces.Persistance;
using Paragraph.Persistence;
using Paragraph.Services.Implementations;

namespace Paragraph.Services
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddServicesRegistration(this IServiceCollection services)
        {
            services.AddScoped<IParagraphDbContext, ParagraphDbContext>();

            services.AddScoped<IAccountService, AccountService>();
            return services;
        }
    }
}