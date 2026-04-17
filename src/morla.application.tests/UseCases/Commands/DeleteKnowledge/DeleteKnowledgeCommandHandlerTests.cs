using Moq;
using Xunit;
using Morla.Application.UseCases.Commands.DeleteKnowledge;
using Morla.Domain.Models;
using Morla.Domain.Repository;

namespace Morla.Application.Tests.UseCases.Commands.DeleteKnowledge;

public class DeleteKnowledgeCommandHandlerTests
{
    private readonly Mock<IKnowledgeRepository> _mockRepository;
    private readonly DeleteKnowledgeCommandHandler _handler;

    public DeleteKnowledgeCommandHandlerTests()
    {
        _mockRepository = new Mock<IKnowledgeRepository>();
        _handler = new DeleteKnowledgeCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidRowKey_ShouldSoftDelete()
    {
        // Arrange
        var rowKey = "test-rowkey-123";
        var knowledge = new Knowledge
        {
            Id = 1,
            RowId = rowKey,
            Title = "Test Knowledge",
            Summary = "Test Summary",
            Content = "Test Content",
            Topic = "test",
            Project = "test-project",
            IsDeleted = false,
            DeletedAt = null
        };

        _mockRepository.Setup(r => r.GetByIdAsync(rowKey))
            .ReturnsAsync(knowledge);

        var command = new DeleteKnowledgeCommand(rowKey, hardDelete: false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Knowledge marked as deleted (soft-delete)", result.Message);
        Assert.True(result.IsSoftDelete);
        
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Knowledge>(k => 
            k.IsDeleted == true && k.DeletedAt != null)), Times.Once);
    }

    [Fact]
    public async Task Handle_WithHardDeleteFlag_ShouldPermanentlyDelete()
    {
        // Arrange
        var rowKey = "test-rowkey-123";
        var knowledge = new Knowledge
        {
            Id = 1,
            RowId = rowKey,
            Title = "Test Knowledge",
            Summary = "Test Summary",
            Content = "Test Content"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(rowKey))
            .ReturnsAsync(knowledge);

        var command = new DeleteKnowledgeCommand(rowKey, hardDelete: true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Knowledge permanently deleted", result.Message);
        Assert.False(result.IsSoftDelete);
        
        _mockRepository.Verify(r => r.DeleteAsync(rowKey), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentRowKey_ShouldReturnNotFound()
    {
        // Arrange
        var rowKey = "non-existent-rowkey";
        _mockRepository.Setup(r => r.GetByIdAsync(rowKey))
            .ReturnsAsync((Knowledge?)null);

        var command = new DeleteKnowledgeCommand(rowKey);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("NOT_FOUND", result.Error);
        
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Knowledge>()), Times.Never);
    }
}
