using PLDGA.Domain.Entities;

namespace PLDGA.Application.Interfaces;

public interface ISiteSettingsService
{
    Task<SiteSettings> GetSettingsAsync();
    Task SaveSettingsAsync(SiteSettings settings);
    Task<int> GetPointsForPlacement(int placement);
}
