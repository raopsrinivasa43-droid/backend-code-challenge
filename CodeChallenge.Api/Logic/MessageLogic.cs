using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Runtime.ConstrainedExecution;

namespace CodeChallenge.Api.Logic
{
    public class MessageLogic : IMessageLogic
    {
        private readonly IMessageRepository _repository;

        public MessageLogic(IMessageRepository repository)
        {
            _repository = repository; 
        }


        private Dictionary<string, string[]> ValidateTitleAndContent(string? title, string? content)
        {
            var errors = new Dictionary<string, string[]>();

            // Title: required, 3–200 chars
            if (string.IsNullOrWhiteSpace(title) || title.Length < 3 || title.Length > 200)
            {
                errors["Title"] = new[] { "Title must be between 3 and 200 characters." };
            }

            // Content: required, 10–1000 chars
            if (string.IsNullOrWhiteSpace(content) || content.Length < 10 || content.Length > 1000)
            {
                errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };
            }

            return errors;
        }


        public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest? request)
        {
            // null check first
            if (request is null)
            {
                var nullErrors = new Dictionary<string, string[]>
                {
                    ["Request"] = new[] { "Request body cannot be null." }
                };
                return new ValidationError(nullErrors);
            }

            // use shared validation helper
            var errors = ValidateTitleAndContent(request.Title, request.Content);
            if (errors.Count > 0)
                return new ValidationError(errors);

            // Title must be unique per organization for active messages
            var messages = await _repository.GetAllByOrganizationAsync(organizationId);
            if (messages.Any(m => m.Title == request.Title && m.IsActive))
                return new Conflict("Title must be unique for this organization.");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.CreateAsync(message);
            return new Created<Message>(message);
        }


        public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest? request)
        {
            // null check first
            if (request is null)
            {
                var nullErrors = new Dictionary<string, string[]>
                {
                    ["Request"] = new[] { "Request body cannot be null." }
                };
                return new ValidationError(nullErrors);
            }

            var message = await _repository.GetByIdAsync(organizationId, id);
            if (message == null)
                return new NotFound("Message not found.");

            // Only active messages can be updated
            if (!message.IsActive)
                return new Conflict("Only active messages can be updated.");

            // use shared validation helper
            var errors = ValidateTitleAndContent(request.Title, request.Content);
            if (errors.Count > 0)
                return new ValidationError(errors);

            // Title must be unique per organization for active messages (except this message)
            var messages = await _repository.GetAllByOrganizationAsync(organizationId);
            if (messages.Any(m => m.Title == request.Title && m.Id != id && m.IsActive))
                return new Conflict("Title must be unique for this organization.");

            message.Title = request.Title;
            message.Content = request.Content;
            message.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(message);
            return new Updated();

        }

        public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
        {
            var message = await _repository.GetByIdAsync(organizationId, id);
            if (message == null)
                return new NotFound("Message not found.");

            // Only active messages can be deleted
            if (!message.IsActive)
                return new Conflict("Only active messages can be deleted.");

            await _repository.DeleteAsync(message.OrganizationId, message.Id);
            return new Deleted();
        }

        public async Task<Result> GetMessageAsync(Guid organizationId, Guid id)
        {
            var message = await _repository.GetByIdAsync(organizationId, id);

            if (message == null)
                return new NotFound("Message not found.");

            return new Success<Message>(message);
        }

        public async Task<Result> GetAllMessagesAsync(Guid organizationId)
        {
            var messages = await _repository.GetAllByOrganizationAsync(organizationId);

            return new Success<IEnumerable<Message>>(messages);
        }
    }
}
