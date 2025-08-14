using Microsoft.AspNetCore.Mvc;
using MeetingScheduler.Models;
using MeetingScheduler.DTOs;
using MeetingScheduler.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MeetingScheduler.Controllers;

[ApiController]
[Route("[controller]")]
public class MeetingsController : ControllerBase
{
    public readonly MeetingsService _meetingsService;

    public MeetingsController()
    {
        _meetingsService = new MeetingsService();
    }

    [HttpGet]
    public async Task<ActionResult<List<Meeting>>> GetAllMeetings()
    {
        var allMeetings = await _meetingsService.GetAllMeetings();
        if (allMeetings == null)
        {
            return NotFound();
        }

        return Ok(allMeetings);
    }
    
    [HttpGet("users/{id:int}/meetings")]
    public async Task<ActionResult<List<Meeting>>> GetAllMeetingsForUser([FromRoute] int id)
    {
        var allMeetings = await _meetingsService.GetAllMeetingsForUser(id);
        if (allMeetings == null)
        {
            return NotFound();
        }

        return Ok(allMeetings);
    }
    
    [HttpPost("meetings")]
    public async Task<ActionResult<Meeting>> CreateMeeting([FromBody] MeetingCreateRequestModel request)
    {
        var meeting = await _meetingsService.CreateMeeting(request);

        if (meeting == null)
            return BadRequest("Meeting creation failed. No free schedule.");

        return CreatedAtAction(nameof(GetMeetingById), new { id = meeting.Id }, meeting);
    }


    [HttpGet("meetings/{id:int}")]
    public ActionResult<Meeting> GetMeetingById([FromRoute]int id)
    {
        var meeting = _meetingsService.GetMeetingById(id);
        if (meeting == null)
            return NotFound();
        return Ok(meeting);
    }
}