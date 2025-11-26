using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageRepository repository, ILogger<MessagesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
    {
        // TODO: Implement
        var messages = await _repository.GetAllByOrganizationAsync(organizationId);
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        // TODO: Implement
        var message = await _repository.GetByIdAsync(organizationId, id);
        if (message == null)
        {
            return NotFound();
        }
        return Ok(message);
    }

    [HttpPost]
    public async Task<ActionResult<Message>> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        // TODO: Implement
        var newMessage = new Message
        {
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        var created = await _repository.CreateAsync(newMessage);
        return CreatedAtAction(nameof(GetById), new { organizationId, id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        // TODO: Implement
        var existing = await _repository.GetByIdAsync(organizationId, id);
        if(existing == null) 
            return NotFound();

        existing.Title = request.Title;
        existing.Content = request.Content;

        var updated = await _repository.UpdateAsync(existing);
        if(updated == null)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        // TODO: Implement
        var deleted = await _repository.DeleteAsync(organizationId, id);
        if(!deleted)
            return NotFound();

        return NoContent();
    }
}
