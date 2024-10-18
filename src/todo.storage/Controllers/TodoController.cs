using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using todo.storage.db;
using todo.storage.model.Requests;
using todo.storage.Services.Todo;
using todo.storage.Services.User;

namespace todo.storage.Controllers;

[ApiController]
[Route("api/[controller]/v1")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class TodoController
{
    private readonly ILogger<TodoController> _logger;
    private readonly ITodoService _todoService;
    private readonly IUserService _userService;

    public TodoController(
        ILogger<TodoController> logger, 
        ITodoService todoService, 
        IUserService userService)
    {
        _logger = logger;
        _todoService = todoService;
        _userService = userService;
    }

    [HttpGet("{userId}")]
    public ActionResult<List<model.Todo>> GetAll([FromRoute] string userId)
    {
        var isValidGuid = Guid.TryParse(userId, out var userGuid);
        if (!isValidGuid)
        {
            return new BadRequestResult();
        }
        var todos = _todoService.FindAllTodos(userGuid);
        
        return todos.Select(todo => todo.ToModelObject()).ToList();
    }
    
    [HttpPost("{userId}")]
    public async Task<ActionResult> RequestPostTodo([FromRoute] string userId, [FromBody] PostTodoRequest req)
    {
        var isValidGuid = Guid.TryParse(userId, out var userGuid);
        var user = await _userService.FindUser(userGuid);
        
        if (user == null || user.ExternalId == Guid.Empty)
        {
            return new BadRequestResult();
        }
        
        var todo = new Todo()
        {
            ExternalId = Guid.NewGuid(),
            UserId = userGuid,
            Name = req.Name, 
            IsComplete = false,
            CompleteDate = DateTime.Now,
            CreatedDate = DateTime.Now
        };
        try
        {
            await _todoService.CreateTodo(todo);
            return new OkResult();
        }
        catch (SystemException e)
        {
            _logger.LogError(e.Message);
            return new BadRequestResult();
        }
    }
    
    [HttpPost("{userId}/completed")]
    public async Task<ActionResult> PostTodoCompleted([FromRoute] string userId, [FromBody] PostTodoCompletedRequest req)
    {
        var isValidGuidUser = Guid.TryParse(userId, out var userGuid);
        if (!isValidGuidUser)
        {
            return new BadRequestResult();
        }
        var user = await _userService.FindUser(userGuid);
        
        if (user == null || user.ExternalId == Guid.Empty)
        {
            return new BadRequestResult();
        }
        
        try
        {
            await _todoService.MarkTodoCompleted(req.TodoId, userGuid);
            return new OkResult();
        }
        catch (SystemException e)
        {
            _logger.LogError(e.Message);
            return new BadRequestResult();
        }
    }
}