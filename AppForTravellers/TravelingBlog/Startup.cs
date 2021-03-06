using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System;
using System.IO;
using System.Text;
using AutoMapper;
using TravelingBlog.ActionFilters;
using TravelingBlog.BusinessLogicLayer.ModelsServices;
using TravelingBlog.BusinessLogicLayer.ModelsServices.Contracts;
using TravelingBlog.BusinessLogicLayer.SecondaryServices.Auth;
using TravelingBlog.BusinessLogicLayer.SecondaryServices.AzureStorage;
using TravelingBlog.BusinessLogicLayer.SecondaryServices.LoggerService;
using TravelingBlog.DataAcceesLayer.Data;
using TravelingBlog.DataAcceesLayer.Models;
using TravelingBlog.DataAcceesLayer.Models.Entities;
using TravelingBlog.Extensions;
using TravelingBlog.Helpers;
using TravelingBlog.Models.AuthModels;

namespace TravelingBlog
{
    public class Startup
    {
        private readonly SymmetricSecurityKey _signingKey;
        
        public Startup(IConfiguration configuration)
        {
            LogManager.LoadConfiguration(String.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            Configuration = configuration;
            _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("JwtIssuerOptions")["SecretKey"]));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureCors();
            services.ConfigureIISIntegration();
            services.ConfigureLoggerService();
            services.ConfigureSqlContext(Configuration);
            services.ConfigureUnitOfWork();
            services.ConfigureAutoMapper();
            services.ConfigureSeed();

            services.AddScoped<ITripService, TripService>();

            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IAdminService, AdminService>();
            //
            services.AddScoped<ISubscriptionService, SubscriptionService>();

            services.AddScoped<ISearchService, SearchService>();

            services.AddScoped<ICountryService, CountryService>();

            services.AddScoped<IAzureBlob, AzureBlob>();

            services.AddScoped<IRatingService, RatingService>();

            services.AddScoped<ISettingsService, SettingsService>();

            services.AddScoped<IPostBlogService, PostBlogService>();

            // Add validation attribute service.
            services.AddScoped<ValidationFilterAttribute>();

            // Add framework services.
            services.AddSingleton<IJwtFactory, JwtFactory>();

            services.Configure<FacebookAuthSettings>(Configuration.GetSection(nameof(FacebookAuthSettings)));
            services.Configure<ReCaptcha>(Configuration.GetSection(nameof(ReCaptcha)));
            services.Configure<BlobConnection>(Configuration.GetSection(nameof(BlobConnection)));

            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();

            services.AddHttpContextAccessor();

            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));
            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
            });

            // api user claim policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser", 
                    policy => 
                    policy
                    .RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess));
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireModeratorAndAdmin", policy => policy.RequireRole("Admin", "Moderator"));
            });

            // add identity
            var builder = services.AddIdentityCore<AppUser>(o =>
            {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            });
            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            builder.AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<AppUser>>();
            services.AddMvc(options=> 
            {
                options.Filters.Add(new ServiceFilterAttribute(typeof(ValidationFilterAttribute)));
            })
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerManager logger, Seed seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureCustomExceptionMiddleware();
            app.UseAuthentication();
            app.UseDefaultFiles();

            app.UseCors("CorsPolicy");

            // app.UseForwardedHeaders will forward proxy headers to the current request. This will help us during the Linux deployment.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            // app.UseStaticFiles() enables using static files for the request.

            app.UseStaticFiles();

            
            app.UseMvc();

            seeder.SeedUsers();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello from api!");
            });

        }
    }
}
