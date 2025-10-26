using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using UserService.API.Middlewares;
using UserService.BO.DTO;
using UserService.Repository;
using UserService.Service;
using UserService.BO.Entities;
using UserService.BO.Enums;
using UserService.API.Configurations;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("DefaultConnection is not configured. Set 'ConnectionStrings:DefaultConnection' in appsettings or environment variable 'ConnectionStrings__DefaultConnection' / 'DEFAULT_CONNECTION'.");
}

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerDependencies();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService.Service.UserService>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // dùng GetService thay vì GetRequiredService để tránh throw khi DI chưa thể resolve
        var context = services.GetService<ApplicationDBContext>();
        if (context == null)
        {
            var loggerNoDb = services.GetService<ILogger<Program>>();
            loggerNoDb?.LogWarning("ApplicationDBContext is not available during seeding. Skipping seeding.");
        }
        else
        {
            bool hasAdmin = false;
            try
            {
                hasAdmin = context.Users.Any(u => u.RoleName == RoleName.ROLE_ADMIN);
            }
            catch (Exception exUsers)
            {
                var loggerUsers = services.GetService<ILogger<Program>>();
                loggerUsers?.LogError(exUsers, "Failed to query Users table during seeding. Skipping admin creation.");
            }

            if (!hasAdmin)
            {
                // lấy hasher nếu có, nếu không thì khởi tạo trực tiếp (tránh phụ thuộc DI nặng)
                var hasher = services.GetService<IPasswordHasher>() ?? new BCryptPasswordHasher();

                var adminEmail = app.Configuration["DefaultAdmin:Email"] ?? Environment.GetEnvironmentVariable("DefaultAdmin__Email") ?? "admin@localhost";
                var adminUserName = app.Configuration["DefaultAdmin:UserName"] ?? Environment.GetEnvironmentVariable("DefaultAdmin__UserName") ?? "admin";
                var adminPassword = app.Configuration["DefaultAdmin:Password"] ?? Environment.GetEnvironmentVariable("DefaultAdmin__Password") ?? "Admin@123";

                var admin = new User
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    Password = hasher.HashPassword(adminPassword),
                    FirstName = "System",
                    LastName = "Administrator",
                    RoleName = RoleName.ROLE_ADMIN,
                    Status = Status.ACTIVE,
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    context.Users.Add(admin);
                    context.SaveChanges();
                }
                catch (Exception exSave)
                {
                    var loggerSave = services.GetService<ILogger<Program>>();
                    loggerSave?.LogError(exSave, "Failed to save admin user during seeding.");
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        logger?.LogError(ex, "An unexpected error occurred while seeding the database.");
    }
}

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAppExceptionHandler();


app.UseMiddleware<JwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
