using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using todo.storage.db;
using todo.storage.model.Queue;
using todo.storage.Services.Queue;

namespace todo.storage.test.Services.Queue;

[TestFixture]
public class QueueServiceTest
{
    private QueueService _queueService;
    private Mock<IAmazonSQS> _sqsClientMock;
    private readonly Mock<ILogger<QueueService>> _loggerMock = new();
    
    [SetUp]
    public void SetUp()
    {
        _sqsClientMock = new Mock<IAmazonSQS>();
        var sqsData = new SqsData();
        _queueService = new QueueService(_sqsClientMock.Object, sqsData, _loggerMock.Object);
    }

    [Test]
    public async Task QueueService_AddCreateUserReqToQueue_SetsCorrectTypeAttribute()
    {
        var sqsResponse = new SendMessageResponse()
        {
            MessageId = "testId",
            HttpStatusCode = System.Net.HttpStatusCode.OK
        };
        _sqsClientMock.Setup(mock => mock.SendMessageAsync(It.IsAny<SendMessageRequest>(), CancellationToken.None))
            .ReturnsAsync(sqsResponse);
        
        var user = new User();
        var createUserStr = MessageTypes.CreateUser.ToString();
        var queueServiceResponse = await _queueService.AddCreateUserReqToQueue(user);
        _sqsClientMock.Verify(
            mock => mock.SendMessageAsync(It.Is<SendMessageRequest>(
                    r => r.MessageAttributes[QueueService.Typekey].StringValue.Equals(createUserStr)
                    ), CancellationToken.None
            ));
        Assert.That(queueServiceResponse, Is.True);
    }
    
    [Test]
    public async Task QueueService_AddCreateUserReqToQueue_SetsCorrectMessage()
    {
        var sqsResponse = new SendMessageResponse()
        {
            MessageId = "testId",
            HttpStatusCode = System.Net.HttpStatusCode.OK
        };
        _sqsClientMock.Setup(mock => mock.SendMessageAsync(It.IsAny<SendMessageRequest>(), CancellationToken.None))
            .ReturnsAsync(sqsResponse);
        
        var userGuid = Guid.NewGuid();
        var user = new User
        {
            ExternalId = userGuid,
            ThirdPartyId = userGuid,
            UserName = "Best",
            FirstName = "User",
            FamilyName = "Ever",
            Email = "yaz",
            CreatedDate = DateTime.Now
        };
        var userJson = JsonConvert.SerializeObject(user);
        var response = await _queueService.AddCreateUserReqToQueue(user);
        _sqsClientMock.Verify(
            mock => mock.SendMessageAsync(It.Is<SendMessageRequest>(
                    r => r.MessageBody.Equals(userJson)
                ), CancellationToken.None
            ));
        Assert.That(response, Is.True);
    }
    
    [Test]
    public async Task QueueService_AddCreateUserReqToQueue_ReturnsFalseForNonOkResponse()
    {
        var sqsResponse = new SendMessageResponse()
        {
            MessageId = "testId",
            HttpStatusCode = System.Net.HttpStatusCode.Accepted
        };
        _sqsClientMock.Setup(mock => mock.SendMessageAsync(It.IsAny<SendMessageRequest>(), CancellationToken.None))
            .ReturnsAsync(sqsResponse);
        
        var user = new User();
        var queueServiceResponse = await _queueService.AddCreateUserReqToQueue(user);
        Assert.That(queueServiceResponse, Is.False);
    }
    
    [Test]
    public async Task QueueService_AddCreateUserReqToQueue_ReturnsFalseWhenSqsThrows()
    {
        _sqsClientMock.Setup(mock => mock.SendMessageAsync(It.IsAny<SendMessageRequest>(), CancellationToken.None))
            .Throws(new Exception());
        
        var user = new User();
        var queueServiceResponse = await _queueService.AddCreateUserReqToQueue(user);
        Assert.That(queueServiceResponse, Is.False);
    }
    
    [Test]
    public async Task QueueService_AddCreateTodoReqToQueue_SetsCorrectTypeAttribute()
    {
        var sqsResponse = new SendMessageResponse()
        {
            MessageId = "testId",
            HttpStatusCode = System.Net.HttpStatusCode.OK
        };
        _sqsClientMock.Setup(mock => mock.SendMessageAsync(It.IsAny<SendMessageRequest>(), CancellationToken.None))
            .ReturnsAsync(sqsResponse);

        var userGuid = Guid.NewGuid();
        var todo = new model.Todo();
        var createTodoStr = MessageTypes.CreateTodo.ToString();
        var queueServiceResponse = await _queueService.AddCreateTodoReqToQueue(userGuid, todo);
        _sqsClientMock.Verify(
            mock => mock.SendMessageAsync(It.Is<SendMessageRequest>(
                    r => r.MessageAttributes[QueueService.Typekey].StringValue.Equals(createTodoStr)
                    ), CancellationToken.None
            ));
        Assert.That(queueServiceResponse, Is.True);
    }
    
    [Test]
    public async Task QueueService_AddCreateTodoReqToQueue_SetsCorrectMessage()
    {
        var sqsResponse = new SendMessageResponse()
        {
            MessageId = "testId",
            HttpStatusCode = System.Net.HttpStatusCode.OK
        };
        _sqsClientMock.Setup(mock => mock.SendMessageAsync(It.IsAny<SendMessageRequest>(), CancellationToken.None))
            .ReturnsAsync(sqsResponse);
        
        var userGuid = Guid.NewGuid();
        var todo = new model.Todo();
        var createTodoQueueMessage = new CreateTodoQueueMessage
        {
            UserId = userGuid,
            Todo = todo
        };
        var messageJson = JsonConvert.SerializeObject(createTodoQueueMessage);
        var response = await _queueService.AddCreateTodoReqToQueue(userGuid, todo);
        _sqsClientMock.Verify(
            mock => mock.SendMessageAsync(It.Is<SendMessageRequest>(
                    r => r.MessageBody.Equals(messageJson)
                ), CancellationToken.None
            ));
        Assert.That(response, Is.True);
    }
    
    [Test]
    public async Task QueueService_AddCreateTodoReqToQueue_ReturnsFalseForNonOkResponse()
    {
        var sqsResponse = new SendMessageResponse()
        {
            MessageId = "testId",
            HttpStatusCode = System.Net.HttpStatusCode.Accepted
        };
        _sqsClientMock.Setup(mock => mock.SendMessageAsync(It.IsAny<SendMessageRequest>(), CancellationToken.None))
            .ReturnsAsync(sqsResponse);
        
        var userGuid = Guid.NewGuid();
        var todo = new model.Todo();
        var queueServiceResponse = await _queueService.AddCreateTodoReqToQueue(userGuid, todo);
        Assert.That(queueServiceResponse, Is.False);
    }
    
    [Test]
    public async Task QueueService_AddCreateTodoReqToQueue_ReturnsFalseWhenSqsThrows()
    {
        _sqsClientMock.Setup(mock => mock.SendMessageAsync(It.IsAny<SendMessageRequest>(), CancellationToken.None))
            .Throws(new Exception());
        
        var userGuid = Guid.NewGuid();
        var todo = new model.Todo();
        var queueServiceResponse = await _queueService.AddCreateTodoReqToQueue(userGuid, todo);
        Assert.That(queueServiceResponse, Is.False);
    }
}