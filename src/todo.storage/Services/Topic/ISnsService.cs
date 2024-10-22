namespace todo.storage.Services.Topic;

public interface ISnsService
{
    public Task PublishUserCreatedNotification(Guid userId);
    public Task PublishTodoCreatedNotification(Guid userId);
}