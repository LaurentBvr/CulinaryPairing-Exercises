using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using CulinaryPairing.Application;
using CulinaryPairing.Domain.Users;
using CulinaryPairing.Infrastructure.Behaviors;
using CulinaryPairing.Infrastructure.Correlation;
using CulinaryPairing.Infrastructure.Database;
using CulinaryPairing.Infrastructure.Database.Interceptors;
using CulinaryPairing.Infrastructure.Jobs;
using Quartz;

namespace CulinaryPairing.Infrastructure;

public static class ServiceCollectionExtensions
{
    static readonly List<Assembly> s_assemblies =
    [
        Assembly.Load("CulinaryPairing.Application"),
        Assembly.Load("CulinaryPairing.Domain")
    ];

    public static IServiceCollection AddCulinaryPairing(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .ConfigureMediator()
            .ConfigureFluentValidation()
            .ConfigureEntityFramework(
                configuration.GetConnectionString("DefaultConnection")!)
            .ConfigureIdentity()
            .ConfigureOutboxJob()
            .ConfigureFeatureFlags(configuration)
            .ConfigureCorrelation();
    }

    static IServiceCollection ConfigureMediator(this IServiceCollection services)
    {
        return services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>),
                typeof(DetailedLoggingBehavior<,>),
                typeof(ValidationBehavior<,>),
                typeof(TransactionBehavior<,>),
                typeof(UnitOfWorkBehavior<,>)
            ];
        });
    }

    static IServiceCollection ConfigureFluentValidation(this IServiceCollection services)
    {
        foreach (var result in AssemblyScanner.FindValidatorsInAssemblies(s_assemblies))
            services.AddTransient(result.InterfaceType, result.ValidatorType);

        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        return services;
    }

    static IServiceCollection ConfigureEntityFramework(
        this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(_ => TimeProvider.System);
        services.AddScoped<AuditableInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<AuditableInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        return services;
    }

    static IServiceCollection ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<CulinaryUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    static IServiceCollection ConfigureOutboxJob(this IServiceCollection services)
    {
        var jobKey = new JobKey(nameof(OutboxMessageJob));

        return services.AddQuartz(configure =>
        {
            configure.AddJob<OutboxMessageJob>(jobKey)
                .AddTrigger(trigger => trigger.ForJob(jobKey)
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInSeconds(10).RepeatForever()));
        })
        .AddQuartzHostedService();
    }

    static IServiceCollection ConfigureFeatureFlags(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));
        return services;
    }

    static IServiceCollection ConfigureCorrelation(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
        return services;
    }
}