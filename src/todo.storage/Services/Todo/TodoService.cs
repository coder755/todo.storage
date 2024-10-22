using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using todo.storage.db;
using todo.storage.model.Exceptions;
using todo.storage.Services.Queue;
using todo.storage.Services.Topic;

namespace todo.storage.Services.Todo;

public class TodoService : ITodoService
{
    private readonly UsersContext _context;
    private readonly IQueueService _queueService;
    private readonly ISnsService _snsService;
    private readonly ILogger<TodoService> _logger;
    
    public TodoService(UsersContext context, IQueueService queueService, ISnsService snsService, ILogger<TodoService> logger)
    {
        _context = context;
        _queueService = queueService;
        _snsService = snsService;
        _logger = logger;
    }
    
    public async Task<db.Todo> CreateTodo(db.Todo todo)
    {
        try
        {
            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
            await _snsService.PublishTodoCreatedNotification(todo.UserId);
            return todo;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e.Message);
            throw new CreateUserException("Unable to save todo to database. Doing nothing");
        }
        catch (Exception e)
        {
            var todoModel = new model.Todo
            {
                ExternalId = todo.ExternalId,
                Name = todo.Name,
                IsComplete = todo.IsComplete,
                CompleteDate = todo.CompleteDate
            };
            await _queueService.AddCreateTodoReqToQueue(todo.UserId, todoModel);
            throw new CreateUserException("Unable to save todo to database. Adding request to queue");
        }
        
    }
    
    private async Task<db.Todo> FindTodo(Guid externalId, Guid userId)
    {
        var todo = await _context.Todos.FirstOrDefaultAsync(td =>
            td.ExternalId == externalId && td.UserId == userId
        );
        return todo;
    }
    
    public IEnumerable<db.Todo> FindAllTodos(Guid userId)
    {
        var todos = _context.Todos.Where(todo => todo.UserId.Equals(userId)).ToList();
        return todos;
    }

    public async Task<db.Todo> MarkTodoCompleted(Guid todoExternalId, Guid userId)
    {
        var foundTodo = await FindTodo(todoExternalId, userId);
        if (foundTodo.ExternalId.Equals(Guid.Empty))
        {
            throw new Exception();
        }

        foundTodo.IsComplete = true;
        foundTodo.CompleteDate = DateTime.Now;

        _context.Todos.Update(foundTodo);
        await _context.SaveChangesAsync();
        return foundTodo;
    }

}