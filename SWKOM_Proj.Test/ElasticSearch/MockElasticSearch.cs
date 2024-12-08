using Nest;

namespace SWKOM_Proj.Test.ElasticSearch;

public class MockElasticSearch
{
    [Fact]
    public async Task CreateIndex_ShouldReturnAcknowledged()
    {
        // Arrange
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("ocr_results");
        var client = new ElasticClient(settings);

        // Act
        var createIndexResponse = await client.Indices.CreateAsync("ocr_results", c => c
            .Map(m => m.AutoMap()));

        // Assert
        Assert.True(createIndexResponse.IsValid);
        Assert.True(createIndexResponse.Acknowledged);
    }

    [Fact]
    public async Task IndexDocument_ShouldReturnCreated()
    {
        // Arrange
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("ocr_results");
        var client = new ElasticClient(settings);

        var document = new
        {
            Id = 1,
            Text = "This is a test OCR result"
        };

        // Act
        var indexResponse = await client.IndexDocumentAsync(document);

        // Assert
        Assert.True(indexResponse.IsValid);
        Assert.Equal("created", indexResponse.Result.ToString().ToLower());
    }

    [Fact]
    public async Task GetDocument_ShouldReturnCorrectDocument()
    {
        // Arrange
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("ocr_results");
        var client = new ElasticClient(settings);

        // Act
        var response = await client.GetAsync<object>(1, g => g.Index("ocr_results"));

        // Assert
        Assert.True(response.Found);
        Assert.Equal(1, ((dynamic)response.Source).Id);
        Assert.Equal("This is a test OCR result", ((dynamic)response.Source).Text);
    }

    [Fact]
    public async Task SearchDocuments_ShouldReturnMatchingResults()
    {
        // Arrange
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("ocr_results");
        var client = new ElasticClient(settings);

        // Act
        var searchResponse = await client.SearchAsync<object>(s => s
            .Query(q => q.MatchAll()));

        // Assert
        Assert.True(searchResponse.IsValid);
        Assert.NotEmpty(searchResponse.Documents);
    }

    [Fact]
    public async Task UpdateDocument_ShouldReturnUpdatedResult()
    {
        // Arrange
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("ocr_results");
        var client = new ElasticClient(settings);

        var updatedDoc = new
        {
            Text = "This text has been updated"
        };

        // Act
        var updateResponse = await client.UpdateAsync<object>(1, u => u
            .Index("ocr_results")
            .Doc(updatedDoc));

        // Assert
        Assert.True(updateResponse.IsValid);
        Assert.Equal("updated", updateResponse.Result.ToString().ToLower());
    }
}