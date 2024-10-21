namespace todo.storage.model.Topic;

public class UserCreatedMessage
{
    public TopicMessageTypes Type = TopicMessageTypes.UserCreated;
    public Guid UserId { get; set; }
}