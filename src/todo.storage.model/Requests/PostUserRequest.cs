using System.Runtime.Serialization;

namespace todo.storage.model.Requests;

[DataContract]
public class PostUserRequest
{
    [DataMember(IsRequired = true)]
    public string ExternalId { get; set; }
    
    [DataMember(IsRequired = true)]
    public string Username { get; set; }
    
    [DataMember(IsRequired = true)]
    public string FirstName { get; set; }
    
    [DataMember(IsRequired = true)]
    public string FamilyName { get; set; }
    
    [DataMember(IsRequired = true)]
    public string Email { get; set; }
}