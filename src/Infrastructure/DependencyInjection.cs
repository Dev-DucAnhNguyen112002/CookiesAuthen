using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Interfaces.Repository;
using CookiesAuthen.Domain.Constants;
using CookiesAuthen.Domain.Entities.Identity;
using CookiesAuthen.Infrastructure.Caching;
using CookiesAuthen.Infrastructure.Data;
using CookiesAuthen.Infrastructure.Data.Interceptors;
using CookiesAuthen.Infrastructure.Data.Persistence.Repositories;
using CookiesAuthen.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("CookiesAuthenDb");
        Guard.Against.Null(connectionString, message: "Connection string 'CookiesAuthenDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });


        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        //builder.Services
        //    .AddIdentityCore<ApplicationUser>()
        //    .AddRoles<IdentityRole>()
        //    .AddEntityFrameworkStores<ApplicationDbContext>()
        //    .AddApiEndpoints();
        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>() 
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddTransient<IPermissionService, PermissionService>();
        builder.Services.AddAuthorization(options => {
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));
           
        });

        builder.Services.addRedisCaching(builder.Configuration);
    }
    public static IServiceCollection addRedisCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            // Lấy chuỗi kết nối từ biến configuration được truyền vào
            var redisConnection = configuration.GetConnectionString("RedisConnection");

            // Kiểm tra null cho chắc chắn (Best practice)
            Guard.Against.Null(redisConnection, message: "Connection string 'RedisConnection' not found.");
            options.Configuration = redisConnection;
            options.InstanceName = "CookiesApp_";
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        return services; // Hợp lệ vì kiểu trả về là IServiceCollection
    }
}
