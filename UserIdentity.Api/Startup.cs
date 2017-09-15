using UserIdentity.DataAccess.Abstract;
using UserIdentity.DataAccess.Concrete;
using UserIdentity.IdentityProvider.Entities;
using UserIdentity.IdentityProvider.Stores;
using UserIdentity.Services.Abstract;
using UserIdentity.Services.Concrete;
using UserIdentity.Services.Models;
using UserIdentity.WebUI.Infrastructure.Identity;
using UserIdentity.WebUI.Infrastructure.Services;
using UserIdentity.WebUI.Infrastructure.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using System;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;

namespace UserIdentity.Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            var builder = new ConfigurationBuilder().SetBasePath(hostingEnvironment.ContentRootPath)
                                        .AddJsonFile("appsettings.json", true, true)
                                        .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                          .AddUserManager<ApplicationUserManager>()
                          .AddRoleManager<ApplicationRoleManager>()
                          .AddSignInManager<ApplicationSignInManager>()
                          .AddDefaultTokenProviders();

            services.AddMemoryCache();

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.Configure<IdentityOptions>(configureOptions =>
            {
                // Password settings.
                configureOptions.Password.RequireDigit = true;
                configureOptions.Password.RequiredLength = 6;
                configureOptions.Password.RequireNonAlphanumeric = true;
                configureOptions.Password.RequireUppercase = false;
                configureOptions.Password.RequireLowercase = true;

                // Lockout settings.
                configureOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                configureOptions.Lockout.MaxFailedAccessAttempts = 6;
                configureOptions.Lockout.AllowedForNewUsers = true;

                // User settings.
                configureOptions.User.RequireUniqueEmail = true;
            });

            // Map appsettings.json file elements to a strongly typed class.
            services.Configure<AppSettings>(Configuration);

            // Add services required for using options.
            services.AddOptions();

            // Add and configure MVC services.
            services.AddMvc().AddJsonOptions(setupAction =>
                              {
                                  // Configure the contract resolver that is used when serializing .NET objects to JSON and vice versa.
                                  setupAction.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                              });

            // Get the connection string from appsettings.json file.
            string connectionString = Configuration.GetConnectionString("UserIdentityDbConnection");

            // Configure custom services to be used by the framework.
            services.AddTransient<IDatabaseConnectionService>(e => new DatabaseConnectionService(connectionString));
            services.AddTransient<IUserStore<ApplicationUser>, UserStore>();
            services.AddTransient<IRoleStore<ApplicationRole>, RoleStore>();
            services.AddTransient<IEmailSender, MessageServices>();
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
            services.AddSingleton<ICacheManagerService, CacheManagerService>();
            services.AddTransient<IUserRepository, UserRepository>();

            services.AddTransient<IEmailService>(e => new EmailService(new SmtpSettings
            {
                From = Configuration["SmtpSettings:From"],
                Host = Configuration["SmtpSettings:Host"],
                Port = int.Parse(Configuration["SmtpSettings:Port"]),
                SenderName = Configuration["SmtpSettings:SenderName"],
                LocalDomain = Configuration["SmtpSettings:LocalDomain"],
                Password = Configuration["SmtpSettings:Password"],
                UserName = Configuration["SmtpSettings:UserName"],
                
            }));

            //services.AddAuthentication().AddJwtBearer(cfg =>
            //{
            //    cfg.RequireHttpsMetadata = false;
            //    cfg.SaveToken = true;

            //    cfg.TokenValidationParameters = new TokenValidationParameters()
            //    {
            //        ValidIssuer = Configuration["Tokens:Issuer"],
            //        ValidAudience = Configuration["Tokens:Issuer"],
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
            //    };

            //});

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My User Idenity API", Version = "v1" });
            });
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (hostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/error/index/500");
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = staticFileResponseContext =>
                {
                    // Configure caching for static files. Files will be cached for 365 days and duration must be provided in seconds.
                    const int maxAge = 365 * 24 * 3600;
                    staticFileResponseContext.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={maxAge}";
                }
            });

            //app.UseAuthentication();
            app.UseStatusCodePagesWithRedirects("/error/index?errorCode={0}");

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvc(routes => { routes.MapRoute("default", "api/{controller}/{action}"); });

          

       
        }
    }
}