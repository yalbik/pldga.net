using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Infrastructure.Persistence;

public class JsonSiteSettingsRepository : ISiteSettingsRepository
{
    private readonly JsonSingleStore<SiteSettings> _store;

    public JsonSiteSettingsRepository(string dataDirectory)
    {
        _store = new JsonSingleStore<SiteSettings>(dataDirectory, "site_settings.json");
    }

    public async Task<SiteSettings> GetAsync()
    {
        return await _store.ReadAsync();
    }

    public async Task SaveAsync(SiteSettings settings)
    {
        await _store.WriteAsync(settings);
    }
}
