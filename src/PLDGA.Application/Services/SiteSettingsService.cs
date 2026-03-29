using PLDGA.Application.Interfaces;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Application.Services;

public class SiteSettingsService : ISiteSettingsService
{
    private readonly ISiteSettingsRepository _settingsRepository;

    public SiteSettingsService(ISiteSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<SiteSettings> GetSettingsAsync()
    {
        return await _settingsRepository.GetAsync();
    }

    public async Task SaveSettingsAsync(SiteSettings settings)
    {
        await _settingsRepository.SaveAsync(settings);
    }

    public async Task<int> GetPointsForPlacement(int placement)
    {
        var settings = await _settingsRepository.GetAsync();
        return settings.PlacementPoints.TryGetValue(placement, out var points) ? points : 0;
    }
}
