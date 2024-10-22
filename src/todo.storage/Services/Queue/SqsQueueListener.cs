using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using todo.storage.model.Queue;
using todo.storage.Services.Todo;
using todo.storage.Services.User;

namespace todo.storage.Services.Queue;

public class SqsQueueListener : IHostedService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _queueUrl;
    private readonly ILogger<SqsQueueListener> _logger;
    private const string Typekey = "Type";

    public SqsQueueListener(IAmazonSQS sqsClient, SqsData sqsData, ILogger<SqsQueueListener> logger, IServiceProvider serviceProvider)
    {
        _sqsClient = sqsClient;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _queueUrl = sqsData.ToProcessQueueUrl;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Do not await!!!
        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 5,
                    VisibilityTimeout = 5,
                    MessageAttributeNames = new List<string> {"All"}
                };

                var response = await _sqsClient.ReceiveMessageAsync(request, cancellationToken);

                foreach (var message in response.Messages)
                {
                    await ProcessMessage(message);
                }
            }
        }, cancellationToken);
    }

    private async Task ProcessMessage(Message message)
    {
        _logger.LogInformation($"Message received: {message.Body}");
        var messageType = GetMessageType(message);
        switch (messageType)
        {
            case MessageTypes.CreateUser:
            {
                await ProcessCreateUserMessage(message);
                break;
            }
            case MessageTypes.CreateTodo:
            {
                await ProcessCreateTodoMessage(message);
                break;
            }
            case MessageTypes.Unknown:
            default:
            {
                _logger.LogError($"Unknown message type received: {message.Body}");
                break;
            }
        };
    }

    private async Task ProcessCreateUserMessage(Message message)
    {
        try
        {
            var user = GetUserFromMessage(message);
            var userToAdd = new db.User()
            {
                ExternalId = user.ExternalId,
                ThirdPartyId = user.ExternalId,
                UserName = user.Username,
                FirstName = user.FirstName,
                FamilyName = user.FamilyName,
                Email = user.Email,
                CreatedDate = DateTime.Now
            };
            
            using (var scope = _serviceProvider.CreateScope())
            {
                // Resolve the scoped service
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                // Use the queue service as needed
                await userService.CreateUser(userToAdd);

            }
            await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
            
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
    
    private async Task ProcessCreateTodoMessage(Message message)
    {
        try
        {
            var request = GetTodoReqFromMessage(message);
            var todoToAdd = new db.Todo()
            {
                ExternalId = request.Todo.ExternalId,
                UserId = request.UserId,
                Name = request.Todo.Name,
                IsComplete = false,
                CompleteDate = DateTime.Now,
                CreatedDate = DateTime.Now,
            };
            
            using (var scope = _serviceProvider.CreateScope())
            {
                // Resolve the scoped service
                var todoService = scope.ServiceProvider.GetRequiredService<ITodoService>();
                // Use the queue service as needed
                await todoService.CreateTodo(todoToAdd);
            }
            await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
            
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
    
    private MessageTypes GetMessageType(Message message)
    {
        var hasTypeKey = message.MessageAttributes.TryGetValue(Typekey, out var value);
        
        if (hasTypeKey)
        {
            var success = Enum.TryParse(value.StringValue, out MessageTypes messageType);
            if (success)
            {
                return messageType;
            }
        }

        return MessageTypes.Unknown;
    }

    private model.User GetUserFromMessage(Message message)
    {
        var user = JsonConvert.DeserializeObject<model.User>(message.Body);
        return user;
    }
    
    private CreateTodoQueueMessage GetTodoReqFromMessage(Message message)
    {
        var todoRequest = JsonConvert.DeserializeObject<CreateTodoQueueMessage>(message.Body);
        return todoRequest;
    }

    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}