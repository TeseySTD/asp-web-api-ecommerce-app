using FluentAssertions;
using JetBrains.Annotations;
using Shared.Core.Auth;
using Shared.Core.Validation.Result;
using Users.Application.Dto.User;
using Users.Application.UseCases.Users.Queries.GetUserById;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Users.Queries;

[TestSubject(typeof(GetUserByIdQueryHandler))]
public class GetUserByIdQueryHandlerTest : IntegrationTest
{
    public GetUserByIdQueryHandlerTest(DatabaseFixture dbFixture) : base(dbFixture)
    {
    }

    [Fact]
    public async Task WhenIdIsNotInDb_ThenReturnsFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var queryHandler = new GetUserByIdQueryHandler(ApplicationDbContext);

        // Act
        var result = await queryHandler.Handle(query, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == Error.NotFound);
    }

    [Fact]
    public async Task WhenUserIsInDb_ThenReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var queryHandler = new GetUserByIdQueryHandler(ApplicationDbContext);

        var user = User.Create(
            id: UserId.Create(userId).Value,
            name: UserName.Create("test").Value,
            email: Email.Create("test@test.com").Value,
            phoneNumber: PhoneNumber.Create("+3809912345678").Value,
            role: UserRole.Default,
            hashedPassword: HashedPassword.Create("test@test.com").Value
        );

        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await queryHandler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Should().Satisfy<UserReadDto>(d =>
        {
            d.Id.Should().Be(userId);
            d.Name.Should().Be(user.Name.Value);
            d.Email.Should().Be(user.Email.Value);
            d.PhoneNumber.Should().Be(user.PhoneNumber.Value);
            d.Role.Should().Be(user.Role.ToString());
        });
    }
}