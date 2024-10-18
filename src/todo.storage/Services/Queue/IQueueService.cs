namespace todo.storage.Services.Queue;

public interface IQueueService
{
    Task<bool> AddCreateUserToQueue(db.User user);
}