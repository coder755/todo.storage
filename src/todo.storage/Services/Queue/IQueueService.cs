
namespace todo.storage.Services.Queue;

public interface IQueueService
{
    Task<bool> AddCreateUserReqToQueue(db.User user);
    Task<bool> AddCreateTodoReqToQueue(Guid userId, model.Todo todo);
}