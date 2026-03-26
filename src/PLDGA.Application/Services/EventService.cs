using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ISeasonRepository _seasonRepository;
    private readonly ISiteSettingsService _settingsService;

    public EventService(
        IEventRepository eventRepository,
        IMemberRepository memberRepository,
        ISeasonRepository seasonRepository,
        ISiteSettingsService settingsService)
    {
        _eventRepository = eventRepository;
        _memberRepository = memberRepository;
        _seasonRepository = seasonRepository;
        _settingsService = settingsService;
    }

    public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
    {
        var events = await _eventRepository.GetAllAsync();
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return events.Select(e => MapToDto(e, members)).OrderByDescending(e => e.Date);
    }

    public async Task<EventDto?> GetEventByIdAsync(Guid id)
    {
        var evt = await _eventRepository.GetByIdAsync(id);
        if (evt == null) return null;
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return MapToDto(evt, members);
    }

    public async Task<IEnumerable<EventDto>> GetUpcomingEventsAsync()
    {
        var events = await _eventRepository.GetByStatusAsync(EventStatus.Upcoming);
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return events.Select(e => MapToDto(e, members)).OrderBy(e => e.Date);
    }

    public async Task<IEnumerable<EventDto>> GetCompletedEventsAsync()
    {
        var events = await _eventRepository.GetByStatusAsync(EventStatus.Completed);
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return events.Select(e => MapToDto(e, members)).OrderByDescending(e => e.Date);
    }

    public async Task<IEnumerable<EventDto>> GetEventsBySeasonAsync(int year)
    {
        var events = await _eventRepository.GetBySeasonAsync(year);
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return events.Select(e => MapToDto(e, members)).OrderByDescending(e => e.Date);
    }

    public async Task<EventDto> CreateEventAsync(CreateEventDto dto, int seasonYear)
    {
        var evt = new Event
        {
            Name = dto.Name,
            Date = dto.Date,
            LocationName = dto.LocationName,
            LocationAddress = dto.LocationAddress,
            CourseInfo = dto.CourseInfo,
            MaxParticipants = dto.MaxParticipants,
            RegistrationDeadline = dto.RegistrationDeadline,
            Status = EventStatus.Upcoming,
            SeasonYear = seasonYear
        };

        await _eventRepository.AddAsync(evt);

        // Add to season
        var season = await _seasonRepository.GetByYearAsync(seasonYear);
        if (season != null)
        {
            season.EventIds.Add(evt.Id);
            await _seasonRepository.UpdateAsync(season);
        }

        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return MapToDto(evt, members);
    }

    public async Task UpdateEventAsync(EventDto dto)
    {
        var evt = await _eventRepository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException("Event not found.");

        evt.Name = dto.Name;
        evt.Date = dto.Date;
        evt.LocationName = dto.LocationName;
        evt.LocationAddress = dto.LocationAddress;
        evt.CourseInfo = dto.CourseInfo;
        evt.MaxParticipants = dto.MaxParticipants;
        evt.RegistrationDeadline = dto.RegistrationDeadline;

        await _eventRepository.UpdateAsync(evt);
    }

    public async Task DeleteEventAsync(Guid id)
    {
        await _eventRepository.DeleteAsync(id);
    }

    public async Task RegisterMemberAsync(Guid eventId, Guid memberId)
    {
        var evt = await _eventRepository.GetByIdAsync(eventId)
            ?? throw new InvalidOperationException("Event not found.");

        if (evt.RegisteredMembers.Contains(memberId))
            throw new InvalidOperationException("Member is already registered.");

        if (evt.RegisteredMembers.Count >= evt.MaxParticipants)
            throw new InvalidOperationException("Event is full.");

        if (DateTime.UtcNow > evt.RegistrationDeadline)
            throw new InvalidOperationException("Registration deadline has passed.");

        evt.RegisteredMembers.Add(memberId);
        await _eventRepository.UpdateAsync(evt);
    }

    public async Task UnregisterMemberAsync(Guid eventId, Guid memberId)
    {
        var evt = await _eventRepository.GetByIdAsync(eventId)
            ?? throw new InvalidOperationException("Event not found.");

        evt.RegisteredMembers.Remove(memberId);
        await _eventRepository.UpdateAsync(evt);
    }

    public async Task RecordResultsAsync(Guid eventId, List<RecordResultDto> results)
    {
        var evt = await _eventRepository.GetByIdAsync(eventId)
            ?? throw new InvalidOperationException("Event not found.");

        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);

        evt.Results.Clear();
        foreach (var r in results)
        {
            var points = 0;
            if (members.TryGetValue(r.MemberId, out var member) && member.IsPaid)
            {
                points = await _settingsService.GetPointsForPlacement(r.Placement);
            }

            evt.Results.Add(new EventResult
            {
                MemberId = r.MemberId,
                Placement = r.Placement,
                Score = r.Score,
                PointsAwarded = points
            });
        }

        await _eventRepository.UpdateAsync(evt);
    }

    public async Task CompleteEventAsync(Guid eventId)
    {
        var evt = await _eventRepository.GetByIdAsync(eventId)
            ?? throw new InvalidOperationException("Event not found.");

        evt.Status = EventStatus.Completed;
        await _eventRepository.UpdateAsync(evt);
    }

    private static EventDto MapToDto(Event e, Dictionary<Guid, Member> members) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Date = e.Date,
        LocationName = e.LocationName,
        LocationAddress = e.LocationAddress,
        CourseInfo = e.CourseInfo,
        MaxParticipants = e.MaxParticipants,
        RegistrationDeadline = e.RegistrationDeadline,
        Status = e.Status,
        RegisteredCount = e.RegisteredMembers.Count,
        SeasonYear = e.SeasonYear,
        Results = e.Results.Select(r => new EventResultDto
        {
            MemberId = r.MemberId,
            MemberName = members.TryGetValue(r.MemberId, out var m) ? m.FullName : "Unknown",
            MemberIsPaid = members.TryGetValue(r.MemberId, out var mp) && mp.IsPaid,
            Placement = r.Placement,
            PointsAwarded = r.PointsAwarded,
            Score = r.Score
        }).OrderBy(r => r.Placement).ToList()
    };
}
