using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FoodTester.Api.Infrastructure.Helpers;
using FoodTester.Api.Utility;
using FoodTester.Api.Utility.Extensions;
using FoodTester.DbContext.Infrastructure;
using FoodTester.Infrastructure.Settings;
using Newtonsoft.Json.Serialization;
using Serilog;
using FoodTester.DbContext.Seeders.Base;

namespace FoodTester.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }
        public IAppSettings Settings { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
            Settings = new AppSettings(configuration);
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // I am literally registering this DB Context here, just so I can use EF Core Identity
            services.AddDbContext<FoodQualityContext>(options => options.UseSqlServer(Configuration.GetConnectionString("FoodTesterDb")));

            services.AddControllers() // we need only controllers for our api now
                .AddNewtonsoftJson(options =>
                {
                    // To prevent "A possible object cycle was detected which is not supported" error
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

                    // To get our property names serialized in the first letter lowercased
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            // Do our magic to load services automatically, and resolve their DI
            services.AutoRegisterServices();

            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins(Settings.ClientBaseUrl)
                        .SetIsOriginAllowed(isOriginAllowed: _ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var logger = new LoggerConfiguration()
                                .Enrich.FromLogContext()
                                .Enrich.WithProperty("ApplicationName", "Food tester application")
                                .WriteTo.Console()
                                .MinimumLevel.Override("  Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
                                .WriteTo.File($"{Settings.BaseFolder}{Settings.LogsFolder}\\food_tester_log.txt", rollingInterval: RollingInterval.Hour);


            if (HostingEnvironment.IsDevelopment())
            {
                SwaggerHelper.ConfigureService(services);

                // If deploying to Azure, I need to add lines below as they are going to enable us to view events on the Azure: https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview?tabs=net
                //var telemetryConfiguration = new TelemetryConfiguration(globConfig.AppInsights.InstrumentationKey);
                //logger.WriteTo.ApplicationInsights(telemetryConfiguration, telemetryConverter: TelemetryConverter.Traces);
            }

            Log.Logger = logger.CreateLogger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("default"); // as seen here: https://stackoverflow.com/a/56984245/4267429 - Ordering matters

            app.UseHttpsRedirection();

            app.UseRouting();

            if (HostingEnvironment.IsDevelopment())
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = "";
                });

                // Seed some test data 
                var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
                using var serviceScope = serviceScopeFactory.CreateScope();
                var context = serviceScope.ServiceProvider.GetService<FoodQualityContext>();
                var seeder = new DatabaseSeeder(context);
                seeder.Seed(true).Wait(); // Only ".Wait()" will work, everything else is bs
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
