using System.Net;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using todo.storage.model;
using todo.storage.model.Queue;

namespace todo.storage.Services.Queue;

public class QueueService : IQueueService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly SqsData _sqsData;
    private readonly ILogger<QueueService> _logger;

    public QueueService(IAmazonSQS sqsClient, SqsData sqsData, ILogger<QueueService> logger)
    {
        _sqsClient = sqsClient;
        _sqsData = sqsData;
        _logger = logger;
    }

    public async Task<bool> AddCreateUserToQueue(db.User user)
    {
        var messageBody = JsonConvert.SerializeObject(user);
        var attributes = new Dictionary<string, MessageAttributeValue>
        {
            {
                "Type", new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = MessageTypes.CreateUser.ToString()
                }
            },
            {
                "UserId", new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = user.ExternalId.ToString()
                }
            }
        };
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = (_sqsData.QueueUrl),
            MessageBody = messageBody,
            MessageAttributes = attributes
        };

        try
        {
            var response = await _sqsClient.SendMessageAsync(sendMessageRequest);
            if (response.HttpStatusCode.Equals(HttpStatusCode.OK))
            {
                _logger.LogInformation($"Message sent with ID: {response.MessageId}");
                return true;
            }
            _logger.LogError($"Error sending message with ID: {response.MessageId}");
        }
        catch (Exception e)
        {
            _logger.LogError($"Error sending message: {e.Message}");
        }
        return false;
    }
}