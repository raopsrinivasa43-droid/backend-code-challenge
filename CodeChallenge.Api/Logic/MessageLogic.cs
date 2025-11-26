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

        public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
        {
            var errors = new Dictionary<string, string[]>();

            // Validation: Title required and length check
            if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 3 || request.Title.Length > 200)
            {
                errors["Title"] = new[] { "Title must be between 3 and 200 characters." };
            }

            // Validation: Content length check
            if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length < 10 || request.Content.Length > 1000)
            {
                errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };
            }

            // If validation errors, return all at once
            if (errors.Count > 0)
                return new ValidationError(errors);

            // Business Rule: Title must be unique per organization for active messages
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


        public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
        {
            var message = await _repository.GetByIdAsync(organizationId, id);
            if (message == null)
                return new NotFound("Message not found.");

            // Only active messages can be updated
            if (!message.IsActive)
                return new Conflict("Only active messages can be updated.");

            var errors = new Dictionary<string, string[]>();

            // Validation: Title required and length check
            if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 3 || request.Title.Length > 200)
                errors["Title"] = new[] { "Title must be between 3 and 200 characters." };

            // Validation: Content length check
            if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length < 10 || request.Content.Length > 1000)
                errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };

            if (errors.Count > 0)
                return new ValidationError(errors);

            // Business Rule: Title must be unique per organization for active messages (except this message)
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

        public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
        {
            return await _repository.GetByIdAsync(organizationId, id);
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
        {
            return await _repository.GetAllByOrganizationAsync(organizationId);
        }
    }
}
