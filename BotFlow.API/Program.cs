using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using BotFlow.Application.Services;
using BotFlow.Application.Interfaces;
using BotFlow.Infrastructure.Data;
using BotFlow.Domain.Entities;
using BotFlow.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ========== إضافة سياسة لتجاوز التحقق من البريد ==========
builder.Services.AddSingleton<EmailVerificationOverride>();

// إضافة الخدمات الأساسية أولاً
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// تكوين CORS - تصحيح الاسم
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

// تكوين قاعدة البيانات - SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// تسجيل الـ Services التطبيقية
ConfigureApplicationServices(builder.Services);

// تكوين Swagger مع إصلاح المشكلة
ConfigureSwagger(builder.Services);

// تكوين المصادقة JWT
ConfigureAuthentication(builder.Services, builder.Configuration);

// ========== تكوين حجم الرفع الأقصى للملفات ==========
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
    options.MemoryBufferThreshold = int.MaxValue;
});

// إضافة Memory Cache
builder.Services.AddMemoryCache();

// إضافة HttpContext Accessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ========== Middleware لتجاوز التحقق من البريد ==========
app.Use(async (context, next) =>
{
    // هذا يجعل النظام يعتبر كل البريد الإلكتروني مفعلاً تلقائياً
    // بدون الحاجة لإرسال إيميلات التحقق أو انتظار التأكيد
    context.Items["SkipEmailVerification"] = true;
    await next();
});

// Ensure database is created/migrated before any queries/seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migration failed.");
    }
}

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BotFlow API V1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
    
    app.UseDeveloperExceptionPage();
    
    // تهيئة قاعدة البيانات مع بيانات تجريبية
    await SeedDatabaseAsync(app);
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// ========== إصلاح مشكلة HTTPS ==========
// تعطيل HTTPS redirection للتطوير
if (app.Environment.IsDevelopment())
{
    // لا نستخدم HTTPS redirection في التطوير
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// ========== التصحيح: استخدم AllowFrontend بدلاً من CorsPolicy ==========
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// ========== إضافة الـ Super Admin Login Endpoint هنا ==========
app.MapPost("/api/auth/super-admin/login", async (
    [FromBody] SuperAdminLoginRequest request,
    [FromServices] IAuthService authService,
    [FromServices] ILogger<Program> logger,
    HttpContext context) =>
{
    try
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        
        // نستخدم نفس الـ AuthService لكن مع تحقق من Super Admin فقط
        var user = await FindUserByEmailAsync(context.RequestServices, request.Email);
        
        if (user == null)
            return Results.Unauthorized();
        
        if (!user.IsActive)
            return Results.Unauthorized();
        
        // تحقق من كلمة المرور
        if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return Results.Unauthorized();
        
        // ⭐⭐ **هنا التعديل المهم**: تحقق من Super Admin فقط
        if (user.Role != UserRole.SuperAdmin)
            return Results.Unauthorized();
        
        // محاكاة تسجيل الدخول الناجح
        var token = GenerateJwtTokenForSuperAdmin(user, builder.Configuration);
        var refreshToken = GenerateRefreshToken();
        
        logger.LogInformation("Super Admin logged in: {Email} from IP: {IpAddress}", request.Email, ipAddress);
        
        return Results.Ok(new
        {
            Success = true,
            Message = "Super Admin login successful",
            Data = new
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = 60 * 24 * 7, // 7 أيام
                IsEmailVerified = user.IsEmailVerified
            }
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during super admin login for {Email}", request.Email);
        return Results.StatusCode(500);
    }
}).WithName("SuperAdminLogin")
  .AllowAnonymous()
  .WithTags("Auth");

// ========== إضافة endpoint لتحقق من الـ Token ==========
app.MapGet("/api/auth/verify-token", (HttpContext context) =>
{
    var claims = context.User.Claims;
    var claimsList = claims.Select(c => new { c.Type, c.Value }).ToList();
    
    return Results.Ok(new
    {
        IsAuthenticated = context.User.Identity?.IsAuthenticated ?? false,
        Claims = claimsList,
        Roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
        CustomRoles = claims.Where(c => c.Type == "role").Select(c => c.Value).ToList()
    });
}).RequireAuthorization()
  .WithTags("Auth");

// ========== إضافة health check للمستخدمين ==========
app.MapGet("/api/super-admin/users/test", async (ApplicationDbContext dbContext) =>
{
    try
    {
        var totalUsers = await dbContext.Users.CountAsync();
        var adminUsers = await dbContext.Users
            .Where(u => u.Role == UserRole.SuperAdmin || u.Role == UserRole.Admin)
            .CountAsync();
        
        return Results.Ok(new
        {
            Status = "Database connected",
            TotalUsers = totalUsers,
            AdminUsers = adminUsers,
            SampleUsers = await dbContext.Users
                .Select(u => new { u.Id, u.Email, u.Role, u.IsActive })
                .Take(5)
                .ToListAsync()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
}).AllowAnonymous()
  .WithTags("Test");

app.MapControllers();

// نقاط النهاية الخاصة
app.MapGet("/", () => "BotFlow API is running! 🚀")
   .AllowAnonymous();

app.MapGet("/api/health", () => Results.Ok(new 
{ 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Service = "BotFlow API",
    Version = "1.0.0",
    Database = "SQLite",
    Environment = app.Environment.EnvironmentName,
    EmailVerification = "Disabled (Auto-verified)",
    CorsPolicy = "AllowFrontend",
    Endpoints = new {
        SuperAdminLogin = "/api/auth/super-admin/login",
        Health = "/api/health",
        Swagger = "/swagger"
    }
})).AllowAnonymous()
   .WithTags("System");

// نقطة النهاية للخطأ
app.Map("/error", () => Results.Problem("An error occurred.", statusCode: 500))
   .AllowAnonymous();

// API Documentation endpoint
app.MapGet("/api/docs", () => Results.Redirect("/swagger"))
   .AllowAnonymous()
   .ExcludeFromDescription();

Console.WriteLine("🚀 BotFlow API started successfully!");
Console.WriteLine($"📡 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"🌍 URL: {app.Urls.FirstOrDefault()}");
Console.WriteLine($"📚 Swagger UI: {app.Urls.FirstOrDefault()}/swagger");
Console.WriteLine($"🔐 Super Admin Login: POST {app.Urls.FirstOrDefault()}/api/auth/super-admin/login");
Console.WriteLine($"🔧 CORS: Enabled for http://localhost:3000");
Console.WriteLine($"✉️  Email Verification: DISABLED (Auto-verified for all users)");

await app.RunAsync();

// ========== دوال التكوين ==========

void ConfigureApplicationServices(IServiceCollection services)
{
    // تسجيل الـ Services الأساسية
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IDashboardService, DashboardService>();
    
    // تسجيل الـ Services الجديدة التي أنشأناها
    services.AddScoped<IAnalyticsService, AnalyticsService>();
    services.AddScoped<IBotsService, BotsService>();
    services.AddScoped<IConversationsService, ConversationsService>();
    services.AddScoped<IPagesService, PagesService>();
    services.AddScoped<ISettingsService, SettingsService>();
    services.AddScoped<ITeamService, TeamService>();
    
    // ========== إضافة الـ Services الجديدة للسوبر أدمن ==========
    services.AddScoped<IKPIService, KPIService>();
    services.AddScoped<IAIDataSourceService, AIDataSourceService>();
    services.AddScoped<IFileService, FileService>();
    
    // إضافة Background Service لتحديث الإحصائيات
    services.AddHostedService<StatisticsBackgroundService>();
    
    // إضافة Logging
    services.AddLogging();
    
    // إضافة HttpClient للاستخدام الخارجي
    services.AddHttpClient();
    
    // إضافة HttpClient مخصص لـ FileService
    services.AddHttpClient("FileService", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["FileStorage:BaseUrl"] ?? "https://storage.botflow.com/");
        client.Timeout = TimeSpan.FromMinutes(5);
        client.DefaultRequestHeaders.Add("User-Agent", "BotFlow-API");
    });
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "BotFlow API", 
            Version = "v1",
            Description = "API system for managing social media pages with chatbots"
        });
        
        // تعريف JWT في Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme. 
                            Enter 'Bearer' [space] and then your token.
                            Example: 'Bearer abc123def456'",
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
                    },
                    Scheme = "Bearer",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
        
        // حل مشكلة تضارب أسماء الأنواع في Swagger
        c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        
        // تعريف Super Admin Login في Swagger بشكل صحيح
        c.MapType<SuperAdminLoginRequest>(() => new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["email"] = new OpenApiSchema { 
                    Type = "string", 
                    Example = new Microsoft.OpenApi.Any.OpenApiString("admin@botflow.com") 
                },
                ["password"] = new OpenApiSchema { 
                    Type = "string", 
                    Example = new Microsoft.OpenApi.Any.OpenApiString("Admin@123456") 
                },
                ["twoFactorCode"] = new OpenApiSchema { 
                    Type = "string", 
                    Example = new Microsoft.OpenApi.Any.OpenApiString("123456") 
                }
            }
        });
        
        // إضافة tags لتنظيم Swagger
        c.TagActionsBy(api => new[] { api.GroupName });
        c.DocInclusionPredicate((name, api) => true);
    });
}

void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("Jwt");
    var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? 
        "BotFlowSuperSecretKey@2024!ChangeThisInProduction123456");
    
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "BotFlow.API",
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"] ?? "BotFlow.Client",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            RequireExpirationTime = true
        };
        
        // ========== إضافة معالجة للـ Events ==========
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });
    
    // تحديد الـ Policies بشكل صحيح
    services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireSuperAdminRole", policy => 
            policy.RequireClaim(ClaimTypes.Role, "SuperAdmin"));
        
        options.AddPolicy("RequireAdminRole", policy => 
            policy.RequireClaim(ClaimTypes.Role, "Admin", "SuperAdmin"));
        
        options.AddPolicy("RequireUserRole", policy => 
            policy.RequireClaim(ClaimTypes.Role, "User", "Admin", "SuperAdmin"));
    });
}

// ========== دوال التهيئة ==========

// دالة لتهيئة قاعدة البيانات ببيانات تجريبية
async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // إنشاء قاعدة البيانات إذا لم تكن موجودة
        await dbContext.Database.EnsureCreatedAsync();
        
        // إضافة مستخدم تجريبي إذا لم يكن هناك مستخدمين
        if (!await dbContext.Users.AnyAsync())
        {
            // إنشاء هاش لكلمة المرور
            CreatePasswordHash("Admin@123456", out byte[] passwordHash, out byte[] passwordSalt);
            
            var adminUser = new BotFlow.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "Super",
                LastName = "Admin",
                Email = "super@botflow.com",
                CompanyName = "BotFlow Inc.",
                PhoneNumber = "+201234567890",
                UserName = "superadmin",
                PasswordHash = Convert.ToBase64String(passwordHash),
                PasswordSalt = Convert.ToBase64String(passwordSalt),
                Role = BotFlow.Domain.Enums.UserRole.SuperAdmin,
                SubscriptionPlan = "Business",
                IsActive = true,
                IsEmailVerified = true,
                IsPhoneVerified = true,
                EmailVerifiedAt = DateTime.UtcNow,
                PhoneVerifiedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            
            dbContext.Users.Add(adminUser);
            
            // إضافة مستخدم عادي للاختبار
            var regularUser = new BotFlow.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                CompanyName = "Test Company",
                PhoneNumber = "+201234567891",
                UserName = "johndoe",
                PasswordHash = Convert.ToBase64String(passwordHash),
                PasswordSalt = Convert.ToBase64String(passwordSalt),
                Role = BotFlow.Domain.Enums.UserRole.User,
                SubscriptionPlan = "Pro",
                IsActive = true,
                IsEmailVerified = true,
                IsPhoneVerified = true,
                EmailVerifiedAt = DateTime.UtcNow,
                PhoneVerifiedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            dbContext.Users.Add(regularUser);
            
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation("✅ Database seeded with users:");
            logger.LogInformation($"   Super Admin: super@botflow.com / Admin@123456");
            logger.LogInformation($"   Regular User: john@example.com / Admin@123456");
            
            // إضافة بيانات تجريبية للخدمات الجديدة
            await SeedDemoDataAsync(dbContext, adminUser.Id);
            
            // إضافة بيانات تجريبية للسوبر أدمن
            await SeedSuperAdminDemoDataAsync(dbContext, adminUser.Id);
        }
        else
        {
            var userCount = await dbContext.Users.CountAsync();
            logger.LogInformation($"✅ Database already has {userCount} users.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error seeding database");
    }
}

// دالة مساعدة لإنشاء هاش كلمة المرور
void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
{
    using var hmac = new System.Security.Cryptography.HMACSHA512();
    passwordSalt = hmac.Key;
    passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
}

// دالة للتحقق من كلمة المرور
bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
{
    try
    {
        var hashBytes = Convert.FromBase64String(storedHash);
        var saltBytes = Convert.FromBase64String(storedSalt);
        
        using var hmac = new System.Security.Cryptography.HMACSHA512(saltBytes);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        
        return computedHash.SequenceEqual(hashBytes);
    }
    catch
    {
        return false;
    }
}

// دالة لتهيئة بيانات تجريبية للخدمات الجديدة
async Task SeedDemoDataAsync(ApplicationDbContext context, Guid adminUserId)
{
    try
    {
        // إنشاء SocialPages مع valid UserId
        if (!await context.SocialPages.AnyAsync())
        {
            var socialPages = new List<SocialPage>
            {
                new SocialPage
                {
                    Id = Guid.NewGuid(),
                    PageName = "Facebook Business Page",
                    PageId = "fb_123456789",
                    Platform = SocialPlatform.Facebook,
                    AccessToken = "demo-fb-token-123",
                    IsConnected = true,
                    IsActive = true,
                    UserId = adminUserId,
                    CreatedAt = DateTime.UtcNow
                },
                new SocialPage
                {
                    Id = Guid.NewGuid(),
                    PageName = "Instagram Profile",
                    PageId = "ig_987654321",
                    Platform = SocialPlatform.Instagram,
                    AccessToken = "demo-ig-token-456",
                    IsConnected = true,
                    IsActive = true,
                    UserId = adminUserId,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            context.SocialPages.AddRange(socialPages);
        }
        
        // إنشاء Bots
        if (!await context.Bots.AnyAsync())
        {
            var bots = new List<Bot>
            {
                new Bot
                {
                    Id = Guid.NewGuid(),
                    Name = "Customer Support Bot",
                    Status = BotStatus.Active.ToString(),
                    Description = "Handles customer inquiries 24/7",
                    UserId = adminUserId,
                    CreatedAt = DateTime.UtcNow
                },
                new Bot
                {
                    Id = Guid.NewGuid(),
                    Name = "Sales Assistant Bot",
                    Status = BotStatus.Active.ToString(),
                    Description = "Helps with product recommendations",
                    UserId = adminUserId,
                    CreatedAt = DateTime.UtcNow
                }
            };
            
            context.Bots.AddRange(bots);
        }
        
        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        var logger = context.GetService<ILogger<Program>>();
        logger?.LogError(ex, "Error seeding demo data");
    }
}

// دالة لتهيئة بيانات تجريبية للسوبر أدمن
async Task SeedSuperAdminDemoDataAsync(ApplicationDbContext context, Guid adminUserId)
{
    try
    {
        // إضافة بيانات AI Data Sources تجريبية
        if (!await context.Set<AIDataSource>().AnyAsync())
        {
            var aiDataSources = new List<AIDataSource>
            {
                new AIDataSource
                {
                    Id = Guid.NewGuid(),
                    Name = "Product Documentation",
                    Type = "Document",
                    Status = "Active",
                    Description = "Product guides and API references",
                    FileType = "pdf",
                    FileSize = 2516582,
                    QueryCount = 1247,
                    DocumentCount = 1,
                    ProgressPercentage = 100,
                    UserId = adminUserId,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };
            
            await context.Set<AIDataSource>().AddRangeAsync(aiDataSources);
        }
        
        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        var logger = context.GetService<ILogger<Program>>();
        logger?.LogError(ex, "Error seeding super admin demo data");
    }
}

// ========== Helper Functions للـ Endpoint ==========
async Task<User?> FindUserByEmailAsync(IServiceProvider services, string email)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    return await dbContext.Users
        .FirstOrDefaultAsync(u => u.Email == email);
}

string GenerateJwtTokenForSuperAdmin(User user, IConfiguration configuration)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"] 
        ?? "BotFlowSuperSecretKey@2024!ChangeThisInProduction123456");

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.GivenName, user.FirstName),
        new Claim(ClaimTypes.Surname, user.LastName),
        new Claim(ClaimTypes.Role, user.Role.ToString()),
        new Claim("role", user.Role.ToString()),
        new Claim("FullName", user.FullName),
        new Claim("Company", user.CompanyName ?? ""),
        new Claim("IsEmailVerified", user.IsEmailVerified.ToString()),
        new Claim("SubscriptionPlan", user.SubscriptionPlan ?? ""),
        new Claim("IsSuperAdmin", "true")
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddDays(7), // 7 أيام
        Issuer = configuration["Jwt:Issuer"] ?? "BotFlow.API",
        Audience = configuration["Jwt:Audience"] ?? "BotFlow.Client",
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key), 
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

string GenerateRefreshToken()
{
    var randomNumber = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);
    return Convert.ToBase64String(randomNumber);
}

// ========== تعريفات الأنواع ==========

public class SuperAdminLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TwoFactorCode { get; set; } = "123456";
}

// ========== Background Service ==========
public class StatisticsBackgroundService : BackgroundService
{
    private readonly ILogger<StatisticsBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public StatisticsBackgroundService(
        ILogger<StatisticsBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Statistics Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var kpiService = scope.ServiceProvider.GetRequiredService<IKPIService>();
                    await kpiService.UpdateSystemStatisticsAsync();
                }

                // تحديث كل ساعة
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error updating statistics in background service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Statistics Background Service is stopping.");
    }
}

// ========== صنف لتجاوز التحقق من البريد ==========
public class EmailVerificationOverride
{
    public bool IsEmailVerified { get; } = true;
    public bool SkipVerification { get; } = true;
}