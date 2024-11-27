using Microsoft.Extensions.DependencyInjection;
using FoodTester.Infrastructure.Services;
using FoodTester.Infrastructure.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FoodTester.Services.AnalysisRequestService;

namespace FoodTester.Api.Utility.Extensions
{
    public static class RegisterServicesExtention
    {
        public static void AutoRegisterServices(this IServiceCollection services)
        {
            // First we need to register ISettings etc. 
            // If we don't, registration of services below is going to fail.
            var appSettingsAssemblies = GetAppSettingsInjectableAssemblies();
            foreach (var assembly in appSettingsAssemblies)
            {
                // This has been commented out, because IOptions now does not require me to manually mark these properties
                // RegisterAppSettingsFromAssembly(services, assembly);
            }

            // After that we register services e.g. data fetching, data processing etc.
            var servicesAssemblies = GetServicesInjectableAssemblies();
            foreach (var assembly in servicesAssemblies)
            {
                RegisterServicesFromAssembly(services, assembly);
            }
        }

        private static IEnumerable<Assembly> GetAppSettingsInjectableAssemblies()
        {
            yield return Assembly.GetAssembly(typeof(IAppSettings)); // Infrastructure
        }

        private static IEnumerable<Assembly> GetServicesInjectableAssemblies()
        {
            yield return Assembly.GetAssembly(typeof(AnalysisRequestService)); // Services
        }

        private static void RegisterAppSettingsFromAssembly(IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name.EndsWith("Settings"))
                {
                    services.AddSingleton(type, typeof(AppSettings));
                }
            }
        }

        private static void RegisterServicesFromAssembly(IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IService).IsAssignableFrom(type))
                {
                    var childTypes =
                        type.Assembly
                            .GetTypes()
                            .Where(t => t.IsClass && t.GetInterface(type.Name) != null);

                    foreach (var childType in childTypes)
                    {
                        services.AddScoped(type, childType);
                    }
                }
            }
        }
    }
}
