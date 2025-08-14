using MeetingScheduler.DTOs;
using MeetingScheduler.Models;
namespace MeetingScheduler.Services;

public class MeetingsService
{
    private const int _overlap = 15; // acceptable overlap in X minutes
    private static int _meetingCount = 0;

    private static readonly List<Meeting> _meetings = new();
    
    public Task<List<Meeting>> GetAllMeetingsForUser(int userId)
    {
        return Task.FromResult(_meetings.FindAll(meeting => meeting.Participants.Contains(userId)));
    }
    
    public Task<List<Meeting>> GetAllMeetings()
    {
        return Task.FromResult(_meetings);
    }
    
    public Meeting? GetMeetingById(int id)
    {
        return _meetings.FirstOrDefault(m => m.Id == id);
    }
    
    public Task<Meeting?> CreateMeeting(MeetingCreateRequestModel request)
    {
        var meeting = TryScheduleMeeting(request);
        if (meeting != null)
        {
            _meetings.Add(meeting);
            SortMeetingsByStartTime();
            return Task.FromResult(meeting);
        }

        return Task.FromResult<Meeting?>(null);
    }
    
    private Meeting CreateMeeting(MeetingCreateRequestModel request, DateTime startTime)
    {
        return new Meeting
        {
            Id = ++_meetingCount,
            Participants = new List<int>(request.ParticipantIds),
            StartTime = startTime,
            EndTime = startTime.AddMinutes(request.DurationMinutes)
        };
    }

    private Meeting? TryScheduleMeeting(MeetingCreateRequestModel request)
    {
        var meeting = FindAvailableSchedule(request);
        if (meeting != null)
            return meeting;

        meeting = FindAvailableSchedule(request, _overlap);
        if (meeting != null)
            return meeting;
        
        return null;
    }

    private Meeting? FindAvailableSchedule(MeetingCreateRequestModel request, int overlap = 0)
    {
        if (!_meetings.Any())
        {
            return CreateMeeting(request, request.EarliestStart);
        }

        var conflictMeetings = GetConflictMeetings(request.ParticipantIds);
    
        var beforeFirst = TryScheduleBeforeFirstMeeting(request, conflictMeetings, overlap);
        if (beforeFirst != null)
            return beforeFirst;

        var betweenSchedules = TryScheduleBetweenMeetings(request, conflictMeetings, overlap);
        if (betweenSchedules != null)
            return betweenSchedules;

        var afterLast = TryScheduleAfterLastMeeting(request, conflictMeetings, overlap);
        if (afterLast != null)
            return afterLast;
    
        return null;
    }

    private List<Meeting> GetConflictMeetings(List<int> participantIds)
    {
        return _meetings
            .Where(m => m.Participants.Any(p => participantIds.Contains(p)))
            .OrderBy(m1 => m1.StartTime)
            .ToList();
    }
    
    private Meeting? TryScheduleBeforeFirstMeeting(MeetingCreateRequestModel request,
        List<Meeting> conflictMeetings, int overlap = 0)
    {
        if (!conflictMeetings.Any())
            return CreateMeeting(request, request.EarliestStart);
        
        if ((conflictMeetings.First().StartTime - request.EarliestStart).TotalMinutes + overlap >= request.DurationMinutes)
        {
            return CreateMeeting(request, request.EarliestStart);
            
        }
        
        return null;
    }
    
    private Meeting? TryScheduleBetweenMeetings(MeetingCreateRequestModel request,
        List<Meeting> conflictMeetings, int overlap = 0)
    {
        for (int i = 0; i < conflictMeetings.Count - 1; i++)
        {
            if ((conflictMeetings[i + 1].StartTime - conflictMeetings[i].EndTime.AddMinutes(-overlap)).TotalMinutes
                >= request.DurationMinutes
                && conflictMeetings[i].EndTime.AddMinutes(-overlap) >= request.EarliestStart
                && conflictMeetings[i].EndTime.AddMinutes(request.DurationMinutes-overlap) <= request.LatestEnd)
            {
                return CreateMeeting(request, conflictMeetings[i].EndTime.AddMinutes(-overlap));
            }
        }

        return null;
    }
    
    private Meeting? TryScheduleAfterLastMeeting(MeetingCreateRequestModel request,
    List<Meeting> conflictMeetings, int overlap = 0)
    {
        if (!conflictMeetings.Any())
            return null;

        if ((request.LatestEnd - conflictMeetings.Last().EndTime).TotalMinutes + overlap >= request.DurationMinutes)
        {
            return CreateMeeting(request, conflictMeetings.Last().EndTime.AddMinutes(-overlap));
        }

        return null;
    }
    
    private void SortMeetingsByStartTime()
    {
        _meetings.Sort((m1, m2) => m1.StartTime.CompareTo(m2.StartTime));
    }
}