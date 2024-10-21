using System.Runtime.Serialization;

namespace todo.storage.model.Topic;

public enum TopicMessageTypes
{
    [EnumMember(Value = "UserCreated")]
    UserCreated,
}