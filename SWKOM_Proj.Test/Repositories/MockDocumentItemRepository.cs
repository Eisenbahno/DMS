using System.Net;
using System.Net.Http.Json;
using Moq;
using ASP_Rest_API.Controllers;
using ASP_Rest_API.DTO;
using AutoMapper;
using DAL.Repositories;
using DAL.Entities;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Moq.Protected;

public class MockDocumentItemRepository
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IMapper> _mockMapper;
    private readonly DocumentController _controller;

    public MockDocumentItemRepository()
    {
        // Mock the HttpClientFactory and Mapper
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockMapper = new Mock<IMapper>();

        // Inject the mocks into the controller
        _controller = new DocumentController(_mockHttpClientFactory.Object, _mockMapper.Object);
    }

     [Fact]
    public async Task Get_ReturnsOkWithItems()
    {
        // Arrange: Mock HttpClient response
        var mockItems = new List<DocumentItem> { new DocumentItem { Id = 1, Name = "Test Document" } };
        var mockDtoItems = new List<DocumentItemDto> { new DocumentItemDto { Id = 1, Name = "Test Document" } };

        var mockClient = new Mock<HttpMessageHandler>();
        mockClient.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(mockItems)
            });

        var httpClient = new HttpClient(mockClient.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _mockMapper.Setup(m => m.Map<IEnumerable<DocumentItemDto>>(mockItems)).Returns(mockDtoItems);

        // Act: Call the controller method
        var result = await _controller.Get() as OkObjectResult;

        // Assert: Check the result
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(mockDtoItems, result.Value);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithItem()
    {
        // Arrange: Mock HttpClient response
        var mockItem = new DocumentItem { Id = 1, Name = "Test Document" };
        var mockDtoItem = new DocumentItemDto { Id = 1, Name = "Test Document" };

        var mockClient = new Mock<HttpMessageHandler>();
        mockClient.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(mockItem)
            });

        var httpClient = new HttpClient(mockClient.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _mockMapper.Setup(m => m.Map<DocumentItemDto>(mockItem)).Returns(mockDtoItem);

        // Act: Call the controller method
        var result = await _controller.GetById(1) as OkObjectResult;

        // Assert: Check the result
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(mockDtoItem, result.Value);
    }

    [Fact]
    public async Task Create_ValidItem_ReturnsCreatedAtAction()
    {
        // Arrange: Mock the creation of a document
        var mockItem = new DocumentItem { Id = 1, Name = "New Document" };
        var mockItemDto = new DocumentItemDto { Id = 1, Name = "New Document" };

        var mockClient = new Mock<HttpMessageHandler>();
        mockClient.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = JsonContent.Create(mockItem)
            });

        var httpClient = new HttpClient(mockClient.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        _mockMapper.Setup(m => m.Map<DocumentItem>(mockItemDto)).Returns(mockItem);

        // Act: Call the controller method
        var result = await _controller.Create(mockItemDto) as CreatedAtActionResult;

        // Assert: Check the result
        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal(mockItemDto, result.Value);
    }
}
