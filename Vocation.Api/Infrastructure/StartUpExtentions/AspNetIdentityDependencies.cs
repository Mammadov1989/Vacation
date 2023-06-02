using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocation.Api.Infrastructure.StartUpExtentions
{
    public static class AspNetIdentityDependencies
    {
        public static IServiceCollection AddAspNetIdentityDependencies(this IServiceCollection services,
          IConfiguration configuration)
        {
            //services.AddTransient<IHrUserStore<ApplicationUser>, UserStore>();
            //services.AddTransient<IHrRoleStore<ApplicationRole>, RoleStore>();
            //services.AddTransient<IUserStore<ApplicationUser>, UserStore>();
            //services.AddTransient<IRoleStore<ApplicationRole>, RoleStore>();

            //services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
            //{
            //    opts.Password.RequireDigit = false;
            //    opts.Password.RequireLowercase = false;
            //    opts.Password.RequireUppercase = true;
            //    opts.Password.RequireNonAlphanumeric = false;
            //    opts.Password.RequiredLength = 7;
            //}).AddUserManager<HrUserManager>().AddRoleManager<HrRoleManager>()
            //    .AddDefaultTokenProviders();

            services.AddAuthentication(opts =>
            {
                opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
               .AddJwtBearer(cfg =>
               {
                   cfg.RequireHttpsMetadata = false;
                   cfg.SaveToken = true;
                   cfg.TokenValidationParameters = new TokenValidationParameters()
                   {
                       // standard configuration
                       ValidIssuer = configuration["Auth:Jwt:Issuer"],
                       IssuerSigningKey = new SymmetricSecurityKey(
                           Encoding.UTF8.GetBytes(configuration["Auth:Jwt:Key"])),
                       ValidAudience = configuration["Auth:Jwt:Audience"],
                       ClockSkew = TimeSpan.Zero,
                       // security switches
                       RequireExpirationTime = true,
                       ValidateIssuer = true,
                       ValidateIssuerSigningKey = true,
                       ValidateAudience = true
                   };
                   cfg.IncludeErrorDetails = true;
                   cfg.Events = new JwtBearerEvents
                   {
                       OnMessageReceived = context =>
                       {
                           var accessToken = context.Request.Headers["Authorization"];

                           var path = context.HttpContext.Request.Path;
                           if ((path.StartsWithSegments("/hubs/operation")))
                           {
                               accessToken = context.Request.Query["access_token"];
                               context.Token = accessToken;
                           }
                           return Task.CompletedTask;
                       },
                       OnAuthenticationFailed = context =>
                       {
                           var te = context.Exception;
                           return Task.CompletedTask;
                       },
                       OnTokenValidated = context =>
                       {

                           return Task.CompletedTask;
                       }
                   };
               }).Services.ConfigureApplicationCookie(
                   options =>
                   {
                       options.ExpireTimeSpan = TimeSpan.FromMinutes(3600);
                   });

            return services;
        }
    }

}
