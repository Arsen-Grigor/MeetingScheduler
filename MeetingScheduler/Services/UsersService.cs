using MeetingScheduler.DTOs;
using MeetingScheduler.Models;
namespace MeetingScheduler.Services;

public class UsersService
{
    private static readonly List<User> _users = new();

    public Task CreateUser(UserCreateRequestModel user)
    {
        _users.Add(new User { Id = user.Id, Name = user.Name });
        return Task.CompletedTask;
    }
    
    public Task<User?> GetUserById(int id)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }
    
    public Task<List<User>> GetAllUsers()
    {
        return Task.FromResult(_users);
    }
}