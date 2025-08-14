namespace MeetingScheduler.Models;

public class Meeting
{
    public int Id { get; init; }
    public List<int> Participants { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}