using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Shared.Core.Validation.FluentValidation;
using Shared.Core.Validation.Result;

namespace Shared.Core.Tests.Validation.FluentValidationTest;

public class CustomValidatorsTest
{
    public record TestVo
    {
        public const string NonPositiveErrorMessage = "Non-positive number";
        public const string NonPositiveErrorDescription = "Value must be > 0";

        public int Value { get; init; }
        private TestVo(int v) => Value = v;
        public static Result<TestVo> Create(int v) =>
            v > 0
                ? Result<TestVo>.Success(new TestVo(v))
                : Result<TestVo>.Failure(new Error(NonPositiveErrorMessage, NonPositiveErrorDescription));
    }

    public class TestDto
    {
        public int Number { get; set; }
    }


    public class TestDtoValidator : AbstractValidator<TestDto>
    {
        public TestDtoValidator()
        {
            RuleFor(x => x.Number)
                .MustBeCreatedWith<TestDto, int, TestVo>(v => TestVo.Create(v));
        }
    }

    
    private readonly TestDtoValidator _validator = new();

    [Fact]
    public void When_NumberIsPositive_ShouldNotHaveError()
    {
        var dto = new TestDto { Number = 5 };

        var result = _validator.TestValidate(dto);

        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveValidationErrorFor(x => x.Number);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void When_NumberIsNonPositive_ShouldHaveError(int invalid)
    {
        var dto = new TestDto { Number = invalid };

        var result = _validator.TestValidate(dto);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Number)
            .WithErrorMessage($" {TestVo.NonPositiveErrorMessage}: {TestVo.NonPositiveErrorDescription}");
    }

    [Fact]
    public void CanContainValidationAfterCustomValidator_ShouldHaveError()
    {
        var customValidator = new InlineValidator<TestDto>();
        var dto = new TestDto { Number = 9 };
        
        customValidator.RuleFor(x => x.Number)
            .MustBeCreatedWith<TestDto, int, TestVo>(v => TestVo.Create(v))
            .GreaterThanOrEqualTo(10).WithMessage("Number must be greater than 10");

        var result = customValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Number)
            .WithErrorMessage("Number must be greater than 10");
    }
}