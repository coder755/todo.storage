using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace todo.storage.model.Queue;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageTypes
{
    [EnumMember(Value = "CreateUser")]
    CreateUser,
    [EnumMember(Value = "CreateTodo")]
    CreateTodo,
    [EnumMember(Value = "Unknown")]
    Unknown,
}