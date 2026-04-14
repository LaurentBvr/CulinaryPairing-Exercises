using System.Text;
using System.Threading.RateLimiting;
using CulinaryPairing.Api.Endpoints;
using CulinaryPairing.Api.Infrastructure;
using CulinaryPairing.Api.Middleware;
using CulinaryPairing.Infrastructure;
using CulinaryPairing.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCulinaryPairing(builder.Configuration)
    .AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddAuthorization();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtConfig = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
    };
});

// E1 : Rate limiting sur /login
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(5);
    });
    options.RejectionStatusCode = 429;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPwa", policy =>
    {
        policy.WithOrigins("http://localhost:5002", "https://localhost:5002")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// D : Configuration cookies Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// A1 : Force HTTPS
app.UseHttpsRedirection();

// A2 : HSTS en production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// E4 : Headers de securite
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler(_ => { });
app.UseCors("AllowPwa");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapDishes();
app.MapAuth();
app.Run();