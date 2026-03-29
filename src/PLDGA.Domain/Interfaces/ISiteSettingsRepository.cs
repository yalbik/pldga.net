using PLDGA.Domain.Entities;

namespace PLDGA.Domain.Interfaces;

public interface ISiteSettingsRepository
{
    Task<SiteSettings> GetAsync();
    Task SaveAsync(SiteSettings settings);
}
