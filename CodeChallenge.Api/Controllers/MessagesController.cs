using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageLogic _logic;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageLogic logic, ILogger<MessagesController> logger)
    {
        _logic = logic;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll([FromRoute] Guid organizationId)
    {
        // TODO: Implement
        var messages = await _logic.GetAllMessagesAsync(organizationId);
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById([FromRoute] Guid organizationId, Guid id)
    {
        // TODO: Implement
        var message = await _logic.GetMessageAsync(organizationId, id);
        if (message == null)
        {
            return NotFound();
        }
        return Ok(message);
    }

    [HttpPost]
    public async Task<ActionResult<Message>> Create([FromRoute] Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        // TODO: Implement
        var result = await _logic.CreateMessageAsync(organizationId, request);

        if (result is Created<Message> created)
        {
            return CreatedAtAction(nameof(GetById), new { organizationId, id = created.Value.Id }, created.Value);
        } else if (result is ValidationError valErr)
        {
            return BadRequest(valErr.Errors);
        } else if (result is Conflict conflict)
        {
            return Conflict(conflict.Message);
        } else
        {
            return StatusCode(500);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update([FromRoute] Guid organizationId, [FromRoute] Guid id, [FromBody] UpdateMessageRequest request)
    {
        // TODO: Implement
        var result = await _logic.UpdateMessageAsync(organizationId, id, request);

        if (result is Updated)
        {
            return NoContent();
        } else if (result is NotFound notFound)
        {
            return NotFound(notFound.Message);
        } else if (result is ValidationError valErr)
        {
            return BadRequest(valErr.Errors);
        } else if (result is Conflict conflict)
        {
            return Conflict(conflict.Message);
        } else
        {
            return StatusCode(500);
        }

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] Guid organizationId, [FromRoute] Guid id)
    {
        // TODO: Implement
        var result = await _logic.DeleteMessageAsync(organizationId, id);

        if (result is Deleted)
        {
            return NoContent(); 
        }
        else if (result is NotFound notFound)
        {
            return NotFound(notFound.Message);
        }
        else if (result is Conflict conflict)
        {
            return Conflict(conflict.Message);
        }
        else
        {
            return StatusCode(500);
        }
    }
}
