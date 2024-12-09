using Nest;

namespace SWKOM_Proj.Test.ElasticSearch;

public class OcrResult
{
    public int Id { get; set; }
    public string Text { get; set; }
}

public class MockElasticSearch
{
    [Fact]
    public async Task CreateIndex_ShouldReturnAcknowledged()
    {
        // Arrange
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("ocr_results");
        var client = new ElasticClient(settings);

        // Clean up: Delete the index if it already exists
        if ((await client.Indices.ExistsAsync("ocr_results")).Exists)
        {
            await client.Indices.DeleteAsync("ocr_results");
        }

        // Act
        var createIndexResponse = await client.Indices.CreateAsync("ocr_results", c => c
            .Map(m => m
                .AutoMap()));

        // Assert
        Assert.True(createIndexResponse.IsValid, "The index creation response is invalid.");
        Assert.True(createIndexResponse.Acknowledged, "The index creation was not acknowledged.");
    }


    [Fact]
    public async Task IndexDocument_ShouldReturnCreated()
    {
        // Arrange
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("ocr_results");
        var client = new ElasticClient(settings);

        var testDocument = new OcrResult
        {
            Id = 1,
            Text = "This is a test OCR result"
        };

        // Ensure the document doesn't exist
        await client.DeleteAsync<OcrResult>(1, d => d.Index("ocr_results"));

        // Act
        var indexResponse = await client.IndexAsync(testDocument, i => i.Id(1).Index("ocr_results"));

        // Assert
        Assert.True(indexResponse.IsValid, "Indexing response is invalid.");
        Assert.Equal("created", indexResponse.Result.ToString().ToLower());
    }


    [Fact]
    public async Task GetDocument_ShouldReturnCorrectDocument()
    {
        // Arrange
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("ocr_results");
        var client = new ElasticClient(settings);

        var testDocument = new OcrResult
        {
            Id = 1,
            Text = "This is a test OCR result"
        };

        // Index the document
        await client.IndexAsync(testDocument, i => i.Id(1).Index("ocr_results"));

        // Act
        var response = await client.GetAsync<OcrResult>(1, g => g.Index("ocr_results"));

        // Assert
        Assert.True(response.Found, "Document was not found.");
        Assert.NotNull(response.Source);
        Assert.Equal(1, response.Source.Id);
        Assert.Equal("This is a test OCR result", response.Source.Text);
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

        var initialDocument = new OcrResult
        {
            Id = 1,
            Text = "This is a test OCR result"
        };

        OcrResult updatedDocument = new OcrResult() { Id = 1, Text = "This text has been updated" };

        // Index the initial document
        await client.IndexAsync(initialDocument, i => i.Id(1).Index("ocr_results"));

        // Act: Update the document
        var updateResponse = await client.UpdateAsync<OcrResult>(1, u => u
            .Index("ocr_results")
            .Doc(updatedDocument));

        // Retrieve the updated document
        var getResponse = await client.GetAsync<OcrResult>(1, g => g.Index("ocr_results"));

        // Assert
        Assert.True(updateResponse.IsValid, "Update response is invalid.");
        Assert.Equal("updated", updateResponse.Result.ToString().ToLower());
        Assert.NotNull(getResponse.Source);
        Assert.Equal("This text has been updated", getResponse.Source.Text);
    }

}