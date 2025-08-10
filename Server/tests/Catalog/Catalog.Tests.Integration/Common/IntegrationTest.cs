using Catalog.Application;
using Catalog.Application.Common.Interfaces;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Catalog.Tests.Integration.Common;

public class IntegrationTest : BaseIntegrationTest, IAsyncLifetime
{
    private DatabaseFixture _databaseFixture;
    protected readonly IApplicationDbContext ApplicationDbContext;

    protected IntegrationTest(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(databaseFixture.ConnectionString)
            .Options;

        var dbContext = new ApplicationDbContext(options);

        ApplicationDbContext = dbContext;
    }

    protected void ConfigureMapster(IImageUrlGenerator? imageUrlGenerator = null)
    {
        if (imageUrlGenerator == null)
        {
            imageUrlGenerator = Substitute.For<IImageUrlGenerator>();
            imageUrlGenerator.GenerateUrl(Arg.Any<ImageId>()).Returns(c => $"http://image/{c.Arg<ImageId>().Value}");
        }

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService<IImageUrlGenerator>().Returns(imageUrlGenerator);
        MapsterConfig.Configure(provider);
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync() => _databaseFixture.ResetAsync();
}