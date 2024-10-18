using Microsoft.EntityFrameworkCore;
using todo.storage.db;
using todo.storage.model.Exceptions;
using todo.storage.Services.Queue;

namespace todo.storage.Services.User;

public class UserService : IUserService
{
    private readonly UsersContext _context;
    private readonly IQueueService _queueService;

    public UserService( UsersContext context, IQueueService queueService)
    {
        _context = context;
        _queueService = queueService;
    }
    
    public async Task<db.User> FindUser(Guid externalId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(user =>
            user.ExternalId == externalId
        );
        return user;
    }

    public async Task<db.User> CreateUser(db.User user)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
    
            return user;
        }
        catch (Exception)
        {
            await _queueService.AddCreateUserToQueue(user);
            throw new CreateUserException();
        }
    }

    public async Task<bool> DeleteUser(Guid externalId)
    {
        var user = await FindUser(externalId);
        if (user == null || user.ExternalId == Guid.Empty)
        {
            throw new Exception("User does not exist");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}