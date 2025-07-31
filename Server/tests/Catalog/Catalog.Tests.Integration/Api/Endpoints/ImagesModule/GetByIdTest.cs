using System.Net;
using Catalog.Application.UseCases.Images.Queries.GetImageById;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Tests.Integration.Common;

namespace Catalog.Tests.Integration.Api.Endpoints.ImagesModule;

public class GetByIdTest : ApiTest
{
    public GetByIdTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture, CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }
    
    private const string RequestUrl = "/api/images";

    [Fact]
    public async Task WhenValidData_ThenReturnsOk()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var image = Image.Create(
            ImageId.Create(imageId).Value,
            FileName.Create("image.jpg").Value,
            ImageData.Create([1, 2, 3]).Value,
            ImageContentType.JPEG
        );
        
        ApplicationDbContext.Images.Add(image);
        await ApplicationDbContext.SaveChangesAsync();
        
        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}/{imageId}");
        var result = await response.Content.ReadAsByteArrayAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(image.Data.Value.SequenceEqual(result));
    }

    [Fact]
    public async Task WhenImageNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var imageId = Guid.NewGuid();

        var expectJson = MakeSystemErrorApiOutput(new GetImageByIdQueryHandler.ImageNotFoundError(imageId));
        
        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}/{imageId}");
        var actualJson = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectJson, actualJson, ignoreAllWhiteSpace: true);
    }
}