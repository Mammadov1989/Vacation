using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vocation.Api.Helpers.StartupDependencies
{
    public static class HealthCheckConfiguration
    {
        public static void AddHealthCheckConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks().AddSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
