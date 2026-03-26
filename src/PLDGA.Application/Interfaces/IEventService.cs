using PLDGA.Application.DTOs;

namespace PLDGA.Application.Interfaces;

public interface IEventService
{
    Task<IEnumerable<EventDto>> GetAllEventsAsync();
    Task<EventDto?> GetEventByIdAsync(Guid id);
    Task<IEnumerable<EventDto>> GetUpcomingEventsAsync();
    Task<IEnumerable<EventDto>> GetCompletedEventsAsync();
    Task<IEnumerable<EventDto>> GetEventsBySeasonAsync(int year);
    Task<EventDto> CreateEventAsync(CreateEventDto dto, int seasonYear);
    Task UpdateEventAsync(EventDto dto);
    Task DeleteEventAsync(Guid id);
    Task RegisterMemberAsync(Guid eventId, Guid memberId);
    Task UnregisterMemberAsync(Guid eventId, Guid memberId);
    Task RecordResultsAsync(Guid eventId, List<RecordResultDto> results);
    Task CompleteEventAsync(Guid eventId);
}
