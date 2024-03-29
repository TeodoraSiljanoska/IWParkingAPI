﻿using FluentValidation.AspNetCore;
using IWParkingAPI.Fluent_Validations;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Middleware.Authentication;
using IWParkingAPI.Middleware.Exceptions;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Services.Implementation;
using IWParkingAPI.Services.Interfaces;
using IWParkingAPI.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using NLog.Web;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics;
using System.Text;

namespace IWParkingAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(SQLRepository<>));
            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IJwtUtils, JwtUtils>();
            services.AddScoped<IJWTDecode, JWTDecode>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IParkingLotService, ParkingLotService>();
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<IZoneService, ZoneService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ICalculateCapacityExtension, CalculateCapacityExtension>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped(typeof(IEnumsExtension<>), typeof(EnumsExtension<>));
            services.AddScoped<ILocalTimeExtension, LocalTimeExtension>();
            services.AddAutoMapper(typeof(Startup));
            services.AddControllers();

           
            services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ParkingDbContextCustom>()
            .AddDefaultTokenProviders();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"

                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                  {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header,

                        },
                        new List<string>()
                      }
                    });
                options.OperationFilter<SecurityRequirementsOperationFilter>();
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "IWParkingAPI", Version = "v1" });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
                options.IncludeErrorDetails = true;
            });

            services.AddCors(o => o.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddFluentValidationAutoValidation();
            services.AddValidator();
          
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ParkingDbContext>(o => o.UseSqlServer(connectionString));
            services.AddDbContext<ParkingDbContextCustom>(options => options.UseSqlServer(connectionString));

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
          //  if (env.IsDevelopment())
          //  {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IWParkingRestApi v1"));
                app.UseCors();
           // }
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseExceptionMiddleware();
            app.UseMiddleware<AuthenticationMiddleware>();
           // app.UseAuthentication();
           // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseStaticFiles();
        }
    }
}