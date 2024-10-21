using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;
using todo.storage.model.Topic;

namespace todo.storage.Services.Topic;

public class SnsService : ISnsService
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly string _topicArn;

    public SnsService(IAmazonSimpleNotificationService snsClient, SnsData snsData)
    {
        _snsClient = snsClient;
        _topicArn = snsData.TopicArn;
    }

    public async Task PublishUserCreatedNotification(Guid userId)
    {
        var message = new TopicMessage
        {
            Type = "UserCreated",
            UserId = userId
        };
        var messageJson = JsonConvert.SerializeObject(message);
        await PublishMessageAsync(messageJson);
    }
    
    private async Task PublishMessageAsync(string message)
    {
        var publishRequest = new PublishRequest
        {
            TopicArn = _topicArn,
            Message = message
        };

        try
        {
            var response = await _snsClient.PublishAsync(publishRequest);
            Console.WriteLine($"Message ID: {response.MessageId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error publishing message: {ex.Message}");
        }
    }
}