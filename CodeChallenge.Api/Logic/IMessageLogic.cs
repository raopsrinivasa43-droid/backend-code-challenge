using CodeChallenge.Api.Models;

namespace CodeChallenge.Api.Logic;

public interface IMessageLogic
{
    Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest? request);
    Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest? request);
    Task<Result> DeleteMessageAsync(Guid organizationId, Guid id);
    Task<Result> GetMessageAsync(Guid organizationId, Guid id);
    Task<Result> GetAllMessagesAsync(Guid organizationId);
}
