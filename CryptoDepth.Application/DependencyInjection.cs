using CryptoDepth.Application.Mappings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace CryptoDepth.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            Type type = typeof(AutoMapperProfile);
            Assembly assembly = type.Assembly;
            services.AddAutoMapper(assembly);

            return services;
        }
    }
}