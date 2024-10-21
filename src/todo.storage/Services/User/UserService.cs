using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using todo.storage.db;
using todo.storage.model.Exceptions;
using todo.storage.Services.Queue;
using todo.storage.Services.Topic;

namespace todo.storage.Services.User;

public class UserService : IUserService
{
    private readonly UsersContext _context;
    private readonly IQueueService _queueService;
    private readonly ISnsService _snsService;
    private readonly ILogger<UserService> _logger;

    public UserService( UsersContext context, IQueueService queueService, ISnsService snsService, ILogger<UserService> logger)
    {
        _context = context;
        _queueService = queueService;
        _snsService = snsService;
        _logger = logger;
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
            await _snsService.PublishUserCreatedNotification(user.ExternalId);
            return user;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e.Message);
            throw new CreateUserException("Unable to save user to database. Doing nothing");
        }
        catch (Exception e)
        {
            await _queueService.AddCreateUserReqToQueue(user);
            throw new CreateUserException("Unable to save user to database. Adding request to queue");
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