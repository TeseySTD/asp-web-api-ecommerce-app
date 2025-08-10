using Catalog.Tests.Integration.Common;

namespace Catalog.Tests.Integration.Application.UseCases.Images.Queries;
using Catalog.Application.UseCases.Images.Queries.GetImageById;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using FluentAssertions;


public class GetImageByIdQueryHandlerTest : IntegrationTest
{
    public GetImageByIdQueryHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Handle_ImageNotInDb_ReturnsImageNotFoundError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var query = new GetImageByIdQuery(nonExistentId);
        var handler = new GetImageByIdQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is GetImageByIdQueryHandler.ImageNotFoundError);
        
        var error = result.Errors.First() as GetImageByIdQueryHandler.ImageNotFoundError;
        error!.Id.Should().Be(nonExistentId);
    }

    [Fact]
    public async Task Handle_ImageExists_ReturnsImage()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var fileName = "test-image.jpg";
        var imageData = new byte[] { 1, 2, 3, 4, 5 };
        var contentType = ImageContentType.JPEG;

        var image = Image.Create(
            FileName.Create(fileName).Value,
            ImageData.Create(imageData).Value,
            contentType
        );
        
        var imageIdValueObject = ImageId.Create(imageId).Value;
        typeof(Image).GetProperty("Id")!.SetValue(image, imageIdValueObject);
        
        ApplicationDbContext.Images.Add(image);
        await ApplicationDbContext.SaveChangesAsync(default);

        var query = new GetImageByIdQuery(imageId);
        var handler = new GetImageByIdQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Value.Should().Be(imageId);
        result.Value.FileName.Value.Should().Be(fileName);
        result.Value.Data.Value.Should().BeEquivalentTo(imageData);
        result.Value.ContentType.Should().Be(contentType);
    }
}
