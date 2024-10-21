namespace todo.storage.Services.Topic;

public interface ISnsService
{
    public Task PublishUserCreatedNotification(Guid userId);
}