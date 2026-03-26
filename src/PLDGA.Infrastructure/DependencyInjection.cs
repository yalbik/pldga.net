using Microsoft.Extensions.DependencyInjection;
using PLDGA.Application.Interfaces;
using PLDGA.Application.Services;
using PLDGA.Domain.Interfaces;
using PLDGA.Infrastructure.Persistence;

namespace PLDGA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dataDirectory)
    {
        // Repositories
        services.AddSingleton<IMemberRepository>(new JsonMemberRepository(dataDirectory));
        services.AddSingleton<IEventRepository>(new JsonEventRepository(dataDirectory));
        services.AddSingleton<IPollRepository>(new JsonPollRepository(dataDirectory));
        services.AddSingleton<INewsArticleRepository>(new JsonNewsArticleRepository(dataDirectory));
        services.AddSingleton<ISeasonRepository>(new JsonSeasonRepository(dataDirectory));
        services.AddSingleton<IUserRepository>(new JsonUserRepository(dataDirectory));
        services.AddSingleton<ISiteSettingsRepository>(new JsonSiteSettingsRepository(dataDirectory));

        // Application Services
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<INewsArticleService, NewsArticleService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISiteSettingsService, SiteSettingsService>();

        return services;
    }
}
