using Microsoft.AspNetCore.Mvc;
using MeetingScheduler.Models;
using MeetingScheduler.DTOs;
using MeetingScheduler.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MeetingScheduler.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    public readonly UsersService _usersService;

    public UsersController()
    {
        _usersService = new UsersService();
    }
    
    [HttpPost("users")]
    public async Task<ActionResult<User>> CreateUser([FromBody] UserCreateRequestModel user)
    {
        await _usersService.CreateUser(user);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }
    
    [HttpGet("users")]
    public async Task<ActionResult<List<User>>> GetAllUsers()
    {
        var allUsers = await _usersService.GetAllUsers();
        if (allUsers == null)
        {
            return NotFound();
        }

        return Ok(allUsers);
    }

    [HttpGet("users/{id:int}")]
    public ActionResult<User> GetUserById([FromRoute]int id)
    {
        var user = _usersService.GetUserById(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }
}