using System.Runtime.Serialization;

namespace todo.storage.model.Requests;

[DataContract]
public class PostTodoCompletedRequest
{
    [DataMember(IsRequired = true)]
    public Guid TodoId { get; set; }
}