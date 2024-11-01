namespace SWKOM_Proj.Tests;

using Moq;
using DAL.Repositories;
using DAL.Entities;
using System.Collections.Generic;
using Xunit;

public class MockDocumentItemRepository
{
    private readonly Mock<IDocumentItemRepository> _mockRepository;

    public MockDocumentItemRepository()
    {
        _mockRepository = new Mock<IDocumentItemRepository>();
    }
    
    [Fact]
    public void GetItemById_ReturnsCorrectItem()
    {
        // Arrange: Mocking the repository behavior
        var mockItem = new DocumentItem { Id = 1, Name = "Test Document" };
        _mockRepository.Setup(repo => repo.GetByIdAsync(1).Result).Returns(mockItem);

        // Act: Call the method
        var result = _mockRepository.Object.GetByIdAsync(1).Result;

        // Assert: Verify the result
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Document", result.Name);
    }
    
    [Fact]
    public void GetAllItems_ReturnsListOfItems()
    {
        // Arrange: Setup mock to return a list of items
        var mockItems = new List<DocumentItem>
        {
            new DocumentItem { Id = 1, Name = "Document 1" },
            new DocumentItem { Id = 2, Name = "Document 2" }
        };
        _mockRepository.Setup(repo => repo.GetAllAsync().Result).Returns(mockItems);

        // Act: Get all items
        var result = _mockRepository.Object.GetAllAsync().Result;

        // Assert: Verify the result
        Assert.Equal(2, result.Count());
        Assert.Equal("Document 1", result.First().Name);
        Assert.Equal("Document 2", result.ElementAt(1).Name);
    }

    [Fact]
    public async Task AddItem_AddsNewItemSuccessfully()
    {
        // Arrange: Mock repository to handle adding an item
        var newItem = new DocumentItem { Id = 3, Name = "New Document" };
        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<DocumentItem>())).ReturnsAsync(true);

        // Act: Add the new item asynchronously
        void result = await _mockRepository.Object.AddAsync(newItem);

        // Assert: Verify that the item was added
        Assert.True(result);
    }

    [Fact]
    public void DeleteItem_RemovesItemSuccessfully()
    {
        // Arrange: Mock repository to handle deleting an item
        _mockRepository.Setup(repo => repo.DeleteAsync(1)).Returns(true);

        // Act: Delete the item
        var result = _mockRepository.Object.DeleteAsync(1);

        // Assert: Verify that the item was deleted
        Assert.True(result);
    }
}