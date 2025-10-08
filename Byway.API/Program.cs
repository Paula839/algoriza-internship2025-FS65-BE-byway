
namespace Byway.API
{
    using Byway.Application.Interfaces;
    using Byway.Application.Services;
    using Byway.Core.Entities;
    using Byway.Core.Interfaces;
    using Byway.Core.Repository;
    using Byway.Core.Settings;
    using Byway.Infrastructure.Authorization;
    using Byway.Infrastructure.Data;
    using Byway.Infrastructure.Seed;
    using Byway.Infrastructure.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Json.Serialization;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers()
                .AddJsonOptions(options => 
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Byway API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...\""
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });


            builder.Services.AddDbContext<BywayDbContext>(options =>
                options.UseLazyLoadingProxies()
                       .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<IRepository<User>, UserRepository>();
            builder.Services.AddScoped<IRepository<Course>, CourseRepository>();
            builder.Services.AddScoped<IRepository<Instructor>, InstructorRepository>();

            builder.Services.AddScoped<PurchaseService>();
            builder.Services.AddScoped<FilterService>();
            
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IEmailService, EmailService>();


            var jwtSection = builder.Configuration.GetSection("Jwt");
            builder.Services.Configure<JwtSettings>(jwtSection);



            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     ValidIssuer = jwtSettings["Issuer"],
                     ValidAudience = jwtSettings["Audience"],
                     IssuerSigningKey = new SymmetricSecurityKey(key),
                     RoleClaimType = ClaimTypes.Role
                 };
             });

            builder.Services.AddAuthorization();

            builder.Services.AddSingleton<IAuthorizationHandler, SameUserHandler>();
            
            var allowedOrigin = "http://localhost:5173";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigin)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); 
                });
            });


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<BywayDbContext>();
                var config = services.GetRequiredService<IConfiguration>();

                if(!context.Users.AsNoTracking().Any())
                {
                   var defaultAdmin = new User
                   {
                       Name = config["DefaultAdmin:Name"],
                       Username = config["DefaultAdmin:Username"],
                       Email = config["DefaultAdmin:Email"],
                       HashedPassword = BCrypt.Net.BCrypt.HashPassword(config["DefaultAdmin:Password"]),
                       IsAdmin = true
                   };

                    context.Users.Add(defaultAdmin);
                    context.SaveChanges();
                }
            }

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BywayDbContext>();
                var seeder = new DatabaseSeeder(db);
                seeder.Seed(userCount: 1000, courseCount: 1000, instructorCount: 300);
            }

                //if (app.Environment.IsDevelopment())
                // For Evaluation :)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");


            app.UseAuthentication();
            app.UseAuthorization();




            app.MapControllers();

            app.Run();
        }
    }
}
