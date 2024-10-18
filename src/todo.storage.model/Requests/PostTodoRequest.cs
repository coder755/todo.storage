using System.Runtime.Serialization;

namespace todo.storage.model.Requests;

[DataContract]
public class PostTodoRequest
{
    [DataMember(IsRequired = true)]
    public string Name { get; set; }
}