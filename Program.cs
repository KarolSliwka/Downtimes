using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using DiscrepancyReport.Services.MessageService;
using DowntimeTracker.Data;
using DowntimeTracker.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace DowntimeTracker
{
    public class Program
    {
        public static bool IsUnderConstruction = false;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);
            var app = builder.Build();
            Configure(app);
            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add services to the container.
            services.AddRazorPages().AddNToastNotifyNoty(new NotyOptions
            {
                ProgressBar = true,
                Timeout = 5000
            });

            // Add ToastNotification
            services.AddNotyf(config =>
            {
                config.DurationInSeconds = 5;
                config.IsDismissable = true;
                config.Position = NotyfPosition.TopRight;
            });

            if (!IsUnderConstruction)
            {
                // Add DbContexts
                services.AddDbContext<TCZNT5000>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("TCZNT5000") ?? throw new InvalidOperationException("Connection string 'TCZNT5000' not found.")));

                services.AddDbContext<TCZNT5000Raptor>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("TCZNT5000Raptor") ?? throw new InvalidOperationException("Connection string 'TCZNT5000Raptor' not found.")));

                services.AddScoped<IClaimsTransformation, ClaimsTransformer>();
                services.AddScoped<IUserService, UserService>();
                services.AddTransient<IAccessService, AccessService>();
                services.AddScoped<MessageServices>();

                // Authentication and Authorization
                services.AddAuthentication(IISDefaults.AuthenticationScheme);
                ConfigureAuthorization(services);

                // Configure Hangfire
                ConfigureHangfire(services, configuration);
            }

            services.AddControllersWithViews();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        private static void ConfigureAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsHrm", policy =>
                {
                    policy.AddAuthenticationSchemes("Windows");
                    policy.RequireRole("isHrm");
                });
                options.AddPolicy("SuperOnly", policy =>
                {
                    policy.AddAuthenticationSchemes("Windows");
                    policy.RequireRole("super");
                });
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.AddAuthenticationSchemes("Windows");
                    policy.RequireRole("super", "admin");
                });
                options.AddPolicy("VisorsOnly", policy =>
                {
                    policy.AddAuthenticationSchemes("Windows");
                    policy.RequireRole("super", "admin", "visor");
                });
                options.AddPolicy("AllUsers", policy =>
                {
                    policy.AddAuthenticationSchemes("Windows");
                    policy.RequireRole("super", "admin", "visor", "user");
                });
            });
        }

        private static void ConfigureHangfire(IServiceCollection services, IConfiguration configuration)
        {
            var hangfireConnectionString = configuration.GetConnectionString("TCZNT5000");

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(hangfireConnectionString, new Hangfire.SqlServer.SqlServerStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 1;
                options.Queues = new[] { "default" };
                options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
            });
        }

        private static void Configure(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                await next();
            });

            app.UseRouting();
            app.UseSession();

            if (!IsUnderConstruction)
            {
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseMiddleware<UserLoginMiddleware>();
                app.UseHangfireDashboard();
                app.MapHangfireDashboard("/hangfire");
            }

            app.UseNToastNotify();
            app.UseNotyf();

            app.UseEndpoints(endpoints =>
            {
                if (IsUnderConstruction)
                {
                    endpoints.MapControllerRoute(
                        name: "maintenance",
                        pattern: "{controller=Maintenance}/{action=Index}/{id?}");
                }
                else
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Downtimes}/{action=Create}/{id?}");
                }
            });

            if (!IsUnderConstruction)
            {
                // Hangfire jobs - run on first day of every 3rd month - quarterly on 1st day of the month
                TimeZoneInfo polandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                RecurringJob.AddOrUpdate<IAccessService>(x => x.CheckUserAccessAsync(), "0 6 1 */3 *", polandTimeZone);
                RecurringJob.AddOrUpdate<IAccessService>(x => x.ChangeUserAccessAsync(), "0 0 * * *", polandTimeZone);
            }
        }
    }
}