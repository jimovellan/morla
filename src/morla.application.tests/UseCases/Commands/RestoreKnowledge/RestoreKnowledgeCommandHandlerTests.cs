using Moq;
using Xunit;
using Morla.Application.UseCases.Commands.RestoreKnowledge;
using Morla.Domain.Models;
using Morla.Domain.Repository;

namespace Morla.Application.Tests.UseCases.Commands.RestoreKnowledge;

public class RestoreKnowledgeCommandHandlerTests
{
    private readonly Mock<IKnowledgeRepository> _mockRepository;
    private readonly RestoreKnowledgeCommandHandler _handler;

    public RestoreKnowledgeCommandHandlerTests()
    {
        _mockRepository = new Mock<IKnowledgeRepository>();
        _handler = new RestoreKnowledgeCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_WithSoftDeletedEntry_ShouldRestore()
    {
        // Arrange
        var rowKey = "test-rowkey-123";
        var deletedAt = DateTime.UtcNow.AddHours(-1);
        var knowledge = new Knowledge
        {
            Id = 1,
            RowId = rowKey,
            Title = "Test Knowledge",
            Summary = "Test Summary",
            Content = "Test Content",
            Topic = "test",
            Project = "test-project",
            IsDeleted = true,
            DeletedAt = deletedAt
        };

        _mockRepository.Setup(r => r.GetByIdIncludingDeletedAsync(rowKey))
            .ReturnsAsync(knowledge);

        var command = new RestoreKnowledgeCommand(rowKey);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Knowledge restored successfully", result.Message);
        Assert.Equal(rowKey, result.RestoredRowKey);
        
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Knowledge>(k => 
            k.IsDeleted == false && k.DeletedAt == null)), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonDeletedEntry_ShouldReturnError()
    {
        // Arrange
        var rowKey = "test-rowkey-123";
        var knowledge = new Knowledge
        {
            Id = 1,
            RowId = rowKey,
            Title = "Test Knowledge",
            IsDeleted = false,
            DeletedAt = null
        };

        _mockRepository.Setup(r => r.GetByIdIncludingDeletedAsync(rowKey))
            .ReturnsAsync(knowledge);

        var command = new RestoreKnowledgeCommand(rowKey);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("NOT_DELETED", result.Error);
        
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Knowledge>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentRowKey_ShouldReturnNotFound()
    {
        // Arrange
        var rowKey = "non-existent-rowkey";
        _mockRepository.Setup(r => r.GetByIdIncludingDeletedAsync(rowKey))
            .ReturnsAsync((Knowledge?)null);

        var command = new RestoreKnowledgeCommand(rowKey);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("NOT_FOUND", result.Error);
        
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Knowledge>()), Times.Never);
    }
}
