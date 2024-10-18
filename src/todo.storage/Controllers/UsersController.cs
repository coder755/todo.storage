using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using todo.storage.db;
using todo.storage.model.Exceptions;
using todo.storage.model.Requests;
using todo.storage.Services.User;

namespace todo.storage.Controllers;

[ApiController]
[Route("api/[controller]/v1")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class UserController
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserService userService,
        ILogger<UserController> logger
        )
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult> RequestPostUser([FromBody] PostUserRequest req)
    {
        var isValidGuid = Guid.TryParse(req.ExternalId, out var userGuid);
        if (!isValidGuid)
        {
            return new BadRequestResult();
        }

        try
        {
            var user = new User()
            {
                ExternalId = userGuid,
                ThirdPartyId = userGuid,
                UserName = req.Username,
                FirstName = req.FirstName,
                FamilyName = req.FamilyName,
                Email = req.Email,
                CreatedDate = DateTime.Now
            };
            await _userService.CreateUser(user);

            return new CreatedResult();
        }
        catch (CreateUserException)
        {
            return new AcceptedResult();
        }
        catch (SystemException e)
        {
            _logger.LogError(e.Message);
            return new BadRequestResult();
        }
    }
    
    [HttpGet("{userId}")]
    public async Task<ActionResult<model.User>> Get([FromRoute] string userId)
    {
        var isValidGuid = Guid.TryParse(userId, out var userGuid);
        if (!isValidGuid)
        {
            return new BadRequestResult();
        }
        var user = await _userService.FindUser(userGuid);
        
        if (user == null || user.ExternalId == Guid.Empty)
        {
            return new NoContentResult();
        }
        
        return user.ToModelObject();
    }
}