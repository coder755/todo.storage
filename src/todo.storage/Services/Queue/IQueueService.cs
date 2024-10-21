
namespace todo.storage.Services.Queue;

public interface IQueueService
{
    Task<bool> AddCreateUserReqToQueue(db.User user);
}