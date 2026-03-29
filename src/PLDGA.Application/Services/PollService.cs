using PLDGA.Application.DTOs;
using PLDGA.Application.Interfaces;
using PLDGA.Domain.Entities;
using PLDGA.Domain.Interfaces;

namespace PLDGA.Application.Services;

public class PollService : IPollService
{
    private readonly IPollRepository _pollRepository;
    private readonly IMemberRepository _memberRepository;

    public PollService(IPollRepository pollRepository, IMemberRepository memberRepository)
    {
        _pollRepository = pollRepository;
        _memberRepository = memberRepository;
    }

    public async Task<IEnumerable<PollDto>> GetAllPollsAsync(Guid? currentMemberId = null)
    {
        var polls = await _pollRepository.GetAllAsync();
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return polls.Select(p => MapToDto(p, members, currentMemberId)).OrderByDescending(p => p.CreatedDate);
    }

    public async Task<PollDto?> GetPollByIdAsync(Guid id, Guid? currentMemberId = null)
    {
        var poll = await _pollRepository.GetByIdAsync(id);
        if (poll == null) return null;
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return MapToDto(poll, members, currentMemberId);
    }

    public async Task<IEnumerable<PollDto>> GetActivePollsAsync(Guid? currentMemberId = null)
    {
        var polls = await _pollRepository.GetByStatusAsync(PollStatus.Active);
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);

        // Auto-close expired polls
        var result = new List<PollDto>();
        foreach (var poll in polls)
        {
            if (poll.EndDate.HasValue && poll.EndDate.Value < DateTime.UtcNow)
            {
                poll.Status = PollStatus.Closed;
                await _pollRepository.UpdateAsync(poll);
                continue;
            }
            result.Add(MapToDto(poll, members, currentMemberId));
        }
        return result.OrderByDescending(p => p.CreatedDate);
    }

    public async Task<PollDto> CreatePollAsync(CreatePollDto dto, Guid createdByMemberId)
    {
        if (dto.Answers.Count < 1 || dto.Answers.Count > 10)
            throw new InvalidOperationException("Polls must have between 1 and 10 answers.");

        var poll = new Poll
        {
            Question = dto.Question,
            CreatedByMemberId = createdByMemberId,
            EndDate = dto.EndDate,
            Answers = dto.Answers.Select((a, i) => new PollAnswer
            {
                Id = i + 1,
                Text = a,
                VoteCount = 0
            }).ToList()
        };

        await _pollRepository.AddAsync(poll);
        var members = (await _memberRepository.GetAllAsync()).ToDictionary(m => m.Id);
        return MapToDto(poll, members, createdByMemberId);
    }

    public async Task VoteAsync(Guid pollId, Guid memberId, int answerId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId)
            ?? throw new InvalidOperationException("Poll not found.");

        if (poll.Status != PollStatus.Active)
            throw new InvalidOperationException("Poll is closed.");

        if (poll.Votes.Any(v => v.MemberId == memberId))
            throw new InvalidOperationException("You have already voted on this poll.");

        var answer = poll.Answers.FirstOrDefault(a => a.Id == answerId)
            ?? throw new InvalidOperationException("Invalid answer.");

        answer.VoteCount++;
        poll.Votes.Add(new PollVote
        {
            MemberId = memberId,
            AnswerId = answerId,
            VotedAt = DateTime.UtcNow
        });

        await _pollRepository.UpdateAsync(poll);
    }

    public async Task ClosePollAsync(Guid pollId)
    {
        var poll = await _pollRepository.GetByIdAsync(pollId)
            ?? throw new InvalidOperationException("Poll not found.");

        poll.Status = PollStatus.Closed;
        await _pollRepository.UpdateAsync(poll);
    }

    public async Task DeletePollAsync(Guid pollId)
    {
        await _pollRepository.DeleteAsync(pollId);
    }

    private static PollDto MapToDto(Poll p, Dictionary<Guid, Member> members, Guid? currentMemberId) 
    {
        var totalVotes = p.Answers.Sum(a => a.VoteCount);
        return new PollDto
        {
            Id = p.Id,
            Question = p.Question,
            CreatedDate = p.CreatedDate,
            CreatedByMemberId = p.CreatedByMemberId,
            CreatedByName = members.TryGetValue(p.CreatedByMemberId, out var m) ? m.FullName : "Unknown",
            Status = p.Status,
            EndDate = p.EndDate,
            TotalVotes = totalVotes,
            HasUserVoted = currentMemberId.HasValue && p.Votes.Any(v => v.MemberId == currentMemberId.Value),
            Answers = p.Answers.Select(a => new PollAnswerDto
            {
                Id = a.Id,
                Text = a.Text,
                VoteCount = a.VoteCount,
                Percentage = totalVotes > 0 ? Math.Round((double)a.VoteCount / totalVotes * 100, 1) : 0
            }).ToList()
        };
    }
}
