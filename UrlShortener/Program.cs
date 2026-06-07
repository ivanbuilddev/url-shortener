using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using UrlShortener.Data;
using UrlShortener.Middleware;
using UrlShortener.Repositories;
using UrlShortener.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using UrlShortener.Sockets;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSignalR();
builder.Services.AddControllers().AddJsonOptions(
    options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("create", config =>
    {
        config.PermitLimit = 10;
        config.Window = TimeSpan.FromMinutes(1);
    });
});

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
var redisAvailable = await IsRedisAvailableAsync(redisConnectionString);

if(redisAvailable)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
    options.Configuration = redisConnectionString;
    options.InstanceName = "urlshortener:";
    });
    Console.WriteLine("Redis connected.");
}
else
{
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("Redis unavailable — using in-memory cache.");
}
var jwtConfig = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorWebApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                             ?? new[] { "http://localhost:5157" };
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=url-shortener.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddHttpClient<IGeolocationService, GeolocationService>();

builder.Services.AddScoped<IClickInfoRepository, ClickInfoRepository>();
builder.Services.AddScoped<IClickInfoService, ClickInfoService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.UseMiddleware<LoggingRequestMiddleware>();
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapHub<DashboardHub>("/hubs/dashboard");
app.MapHub<ClickInfoHub>("/hubs/clickinfo");

app.UseHttpsRedirection();
app.UseCors("BlazorWebApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var addresses = app.Urls;
    foreach (var address in addresses)
    {
        app.Logger.LogInformation("Scalar disponible en: {Address}/scalar/v1", address);
    }
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();


static async Task<bool> IsRedisAvailableAsync(string? connectionString)
{
    if (string.IsNullOrEmpty(connectionString)) return false;
    try
    {
        var redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
        return redis.IsConnected;
    }
    catch { return false; }
}