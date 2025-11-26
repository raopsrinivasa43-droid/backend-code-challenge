using Xunit;
using Moq;
using FluentAssertions;
using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Repositories;
using CodeChallenge.Api.Models;

namespace CodeChallenge.Tests
{
    public class MessageLogicTests
    {
        private readonly Mock<IMessageRepository> _repoMock;
        private readonly MessageLogic _sut;
        private readonly Guid _orgId = Guid.NewGuid();

        public MessageLogicTests()
        {
            _repoMock = new Mock<IMessageRepository>();
            _sut = new MessageLogic(_repoMock.Object);
        }


        // 1. Test successful creation of a message
        [Fact]
        public async Task CreateMessageAsync_ShouldReturnCreated_WhenRequestIsValidAndTitleIsUnique()
        {
            // Arrange
            var request = new CreateMessageRequest
            {
                Title = "Valid title",
                Content = "This is some valid content."
            };

            _repoMock
                .Setup(r => r.GetAllByOrganizationAsync(_orgId))
                .ReturnsAsync(new List<Message>());

            // Act
            var result = await _sut.CreateMessageAsync(_orgId, request);

            // Assert
            result.Should().BeOfType<Created<Message>>();
            var created = result as Created<Message>;
            created!.Value.Title.Should().Be(request.Title);
            created.Value.Content.Should().Be(request.Content);
            created.Value.OrganizationId.Should().Be(_orgId);

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Once);
        }



        // 2. Test duplicate title returns Conflict
        [Fact]
        public async Task CreateMessageAsync_ShouldReturnConflict_WhenTitleIsDuplicateForActiveMessage()
        {
            // Arrange
            var existingMessage = new Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = _orgId,
                Title = "Same title",
                Content = "Existing content",
                IsActive = true
            };

            var request = new CreateMessageRequest
            {
                Title = "Same title",
                Content = "New valid content for duplicate title case."
            };

            _repoMock
                .Setup(r => r.GetAllByOrganizationAsync(_orgId))
                .ReturnsAsync(new List<Message> { existingMessage });

            // Act
            var result = await _sut.CreateMessageAsync(_orgId, request);

            // Assert
            result.Should().BeOfType<Conflict>();
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Never);
        }



        // 3. Test invalid content length returns ValidationError
        [Fact]
        public async Task CreateMessageAsync_ShouldReturnValidationError_WhenContentLengthIsInvalid()
        {
            // Arrange
            var request = new CreateMessageRequest
            {
                Title = "Valid title",
                Content = "Short" // Less than 10 characters
            };

            // Act
            var result = await _sut.CreateMessageAsync(_orgId, request);

            // Assert
            result.Should().BeOfType<ValidationError>();
            var validationError = result as ValidationError;
            validationError!.Errors.Should().ContainKey("Content");
            validationError.Errors["Content"].Should().Contain("Content must be between 10 and 1000 characters.");

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Never);
        }



        // 4. Test update of non-existent message returns NotFound
        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var request = new UpdateMessageRequest
            {
                Title = "Updated title",
                Content = "Updated content with enough characters."
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(_orgId, nonExistentId))
                .ReturnsAsync((Message?)null);

            // Act
            var result = await _sut.UpdateMessageAsync(_orgId, nonExistentId, request);

            // Assert
            result.Should().BeOfType<NotFound>();
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Message>()), Times.Never);
        }



        // 5. Test update of inactive message returns ValidationError (actually Conflict in your logic)
        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnConflict_WhenMessageIsInactive()
        {
            // Arrange
            var inactiveMessage = new Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = _orgId,
                Title = "Inactive message",
                Content = "Some content here for testing.",
                IsActive = false
            };

            var request = new UpdateMessageRequest
            {
                Title = "Updated title",
                Content = "Updated content with valid length."
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(_orgId, inactiveMessage.Id))
                .ReturnsAsync(inactiveMessage);

            // Act
            var result = await _sut.UpdateMessageAsync(_orgId, inactiveMessage.Id, request);

            // Assert
            result.Should().BeOfType<Conflict>();
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Message>()), Times.Never);
        }



        // 6. Test delete of non-existent message returns NotFound
        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _repoMock
                .Setup(r => r.GetByIdAsync(_orgId, nonExistentId))
                .ReturnsAsync((Message?)null);

            // Act
            var result = await _sut.DeleteMessageAsync(_orgId, nonExistentId);

            // Assert
            result.Should().BeOfType<NotFound>();
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

    }
}
