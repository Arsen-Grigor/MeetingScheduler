namespace MeetingScheduler.DTOs;

public class MeetingCreateRequestModel
{
    public List<int> ParticipantIds { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime EarliestStart { get; set; }
    public DateTime LatestEnd { get; set; }
}