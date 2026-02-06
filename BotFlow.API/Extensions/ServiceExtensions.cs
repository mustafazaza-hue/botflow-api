using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace BotFlow.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // إعداد Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "BotFlow API", 
                    Version = "v1",
                    Description = "API for managing social media pages with AI chatbots"
                });
                
                // تعريف مصادقة JWT في Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
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
                        new List<string>()
                    }
                });
            });

            // مصادقة JWT
            var jwtSettings = configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? 
                "BotFlowSecretKey1234567890!@#$%^&*()");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // إعداد السياسات (Policies) للصلاحيات
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => 
                    policy.RequireRole("SuperAdmin", "Admin"));
                
                options.AddPolicy("RequireSuperAdminRole", policy => 
                    policy.RequireRole("SuperAdmin"));
                
                options.AddPolicy("RequireUserRole", policy => 
                    policy.RequireRole("User", "Admin", "SuperAdmin"));
            });

            // إضافة Memory Cache
            services.AddMemoryCache();

            // إضافة Response Compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // إضافة HttpContext Accessor
            services.AddHttpContextAccessor();

            // تسجيل HttpClient للاستخدام الخارجي
            services.AddHttpClient("DefaultClient");
        }
    }
}