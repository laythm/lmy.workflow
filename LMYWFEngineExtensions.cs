using LMY.Workflow.SQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMY.Workflow
{
    public class LMYWFEngineOptions
    {
        public string DBConnectionString { get; set; }
        public string WorkFlowsConfigFilePath { get; set; }
    }

    public static class LMYWFEngineExtensions
    {
        //default dbWrapper
        public static IServiceCollection AddLMYWFEngineMSSQL(this IServiceCollection services)
        {
            services.AddSingleton<ILMYWFEngineDBWrapper, LMYWFEngineMSSQLDBWrapper>();

            return services;
        }

        public static IServiceCollection AddLMYWFEngine(this IServiceCollection services)
        {
            services.AddSingleton<ILMYWFEngine, LMYWFEngine>();

            return services;
        }

        public static void UseLMYWFEngine(this IApplicationBuilder app, Action<LMYWFEngineOptions> configureOptions)
        {
            var lmyWFEngine = app.ApplicationServices.GetRequiredService<ILMYWFEngine>();
            var lmyWFEngineOptions = new LMYWFEngineOptions();
            configureOptions(lmyWFEngineOptions);

            lmyWFEngine.Configure(lmyWFEngineOptions.WorkFlowsConfigFilePath, lmyWFEngineOptions.DBConnectionString);
        }
        public static void UseLMYWFEngine(this IHost host, Action<LMYWFEngineOptions> configureOptions)
        {
            var lmyWFEngine = host.Services.GetRequiredService<ILMYWFEngine>();
            var lmyWFEngineOptions = new LMYWFEngineOptions();
            configureOptions(lmyWFEngineOptions);

            lmyWFEngine.Configure(lmyWFEngineOptions.WorkFlowsConfigFilePath, lmyWFEngineOptions.DBConnectionString);
        }

    }
}
