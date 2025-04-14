using System.ComponentModel.Design;
using System.Resources;
using AutoMapper.EquivalencyExpression;
using Microsoft.Extensions.DependencyInjection;
using Kelimedya.Core.Interfaces.Persistance;
using Kelimedya.Persistence;
using Kelimedya.Services.Implementations;

namespace Kelimedya.Services
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddServicesRegistration(this IServiceCollection services)
        {
            services.AddScoped<IKelimedyaDbContext, KelimedyaDbContext>();

            return services;
        }
    }
}