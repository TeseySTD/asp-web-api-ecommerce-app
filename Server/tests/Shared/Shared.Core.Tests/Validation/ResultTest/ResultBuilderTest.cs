using System.Diagnostics;
using FluentAssertions;
using Shared.Core.Validation.Result;

namespace Shared.Core.Tests.Validation.ResultTest;

public class ResultBuilderTest
{
    private const int NumberOfPreformanceChecks = 100000;
    private const int MaxReasonablePreformanceTimeMs = 75;
    
    private static Result CreateFailure() => Result.Failure(new Error("E", "M"));
    
    [Fact]
    public void Ctor_WithFailureResult_ThrowsArgumentException()
    {
        // Arrange & Act
        Action act = () => new ResultBuilder<Result>(CreateFailure());
        
        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be created with failed result*");
    }

    [Fact]
    public void Check_ErrorCondition_FailsResult()
    {
        // Arrange & Act
        var result = Result.Try()
            .Check(true, new Error("msg", "desc"))
            .Build();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "msg" && e.Description == "desc");
    }

    [Fact]
    public void Check_ErrorConditionFalse_KeepsSuccess()
    {
        // Arrange & Act
        var result = Result.Try()
            .Check(false, new Error("m", "d"))
            .Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Check_ResultWithSuccess_PropagatesSuccess()
    {
        // Arrange & Act
        var result = Result.Try()
            .Check(Result.Success())
            .Build();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
    }


    [Fact]
    public void Check_ResultWithFailure_PropagatesFailure()
    {
        // Arrange & Act
        var result = Result.Try()
            .Check(CreateFailure())
            .Build();
        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Check_ResultWithNestedFailure_PropagatesFailure()
    {
        // Arrange & Act
        var result = Result.Try()
            .Check(
                Result.Try()
                    .Check(true, new Error("m", "d"))
                    .Check(true, new Error("m2", "d2"))
                    .Check(true, new Error("m3", "d3"))
                .Build()
            )
            .Build();
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should()
            .ContainSingle(e => e.Message == "m" && e.Description == "d").And
            .ContainSingle(e => e.Message == "m2" && e.Description == "d2").And
            .ContainSingle(e => e.Message == "m3" && e.Description == "d3");
    }

    [Fact]
    public void Check_FuncWithSuccess_KeepsSuccess()
    {
        // Arrange & Act
        var result = Result.Try()
            .Check(() => false, new Error("m", "d"))
            .Build();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void Check_FuncWithFailure_PropagatesFailure()
    {
        // Arrange & Act
        var result = Result.Try()
            .Check(() => true, new Error("m", "d"))
            .Build();
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "m" && e.Description == "d");
    }
    
    
    [Fact]
    public void CheckIf_IfConditionTrue_EvaluatesCheck()
    {
        // Arrange & Act
        var result = Result.Try()
            .CheckIf(true, true, new Error("X", "Y"))
            .Build();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.First().Message.Should().Be("X");
    }
    
    [Fact]
    public void CheckIf_IfConditionFalse_SkipsCheck()
    {
        // Arrange & Act
        var result = Result.Try()
            .CheckIf(false, true, new Error("X", "Y"))
            .Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CheckIf_IfConditionFalse_SkipsCheckFuncCall()
    {
        // Arrange
        var trigger = false;

        // Act
        var result = Result.Try()
            .CheckIf(false, () =>
            {
                trigger = true;
                return trigger;
            }, new Error("X", "Y"))
            .Build();

        // Assert
        result.IsSuccess.Should().BeTrue();
        trigger.Should().BeFalse();
    }

    [Fact]
    public void Combine_MultipleFailures_AccumulatesErrors()
    {
        // Arrange 
        var r1 = Result.Failure(new Error("A", "1"));
        var r2 = Result.Failure(new Error("B", "2"));
        var r3 = Result.Success();

        // Act
        var result = Result.Try()
            .Combine(r1, r2, r3)
            .Build();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "A")
            .And.Contain(e => e.Message == "B");
    }

    [Fact]
    public void DropIfFail_WhenCalled_StopsFurtherChecks()
    {
        // Arrange & Act
        var result = Result.Try()
            .Check(true, new Error("E", "1"))
            .DropIfFail()
            .Check(true, new Error("E2", "2"))
            .Build();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "E");
    }

    [Fact]
    public void DropIfFail_WhenCalled_StopsFuncCalls()
    {
        // Arrange 
        bool triggered = false;
        
        // Act
        var result = Result.Try()
            .Check(true, new Error("E", "1"))
            .DropIfFail()
            .Check(() =>
            {
                triggered = true;
                return triggered;
            }, new Error("E2", "2"))
            .Build();

        // Assert
        result.IsFailure.Should().BeTrue();
        triggered.Should().BeFalse();
    }

    [Fact]
    public async Task AsyncChain_SuccessScenario_BuildsSuccessfully()
    {
        // Arrange & Act
        var result = await Result.Try()
            .CheckAsync(async () => await Task.FromResult(false), "Error", "Desc")
            .Check(false, "Another error", "Desc")
            .BuildAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AsyncChain_FailureScenario_BuildsFailure()
    {
        // Arrange
        var result = await Result.Try()
            .CheckAsync(async () => await Task.FromResult(false), "Error1", "Desc1")
            .CheckAsync(async () => await Task.FromResult(true), "Error2", "Desc2")
            .BuildAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Error2");
    }

    [Fact]
    public async Task CheckIfAsync_IfCondittionTrue_SkipsFuncCalls()
    {
        // Arrange
        var triggered = false;
        
        // Act
        var result = await Result.Try()
            .CheckIfAsync(false, 
                async () =>
                {
                    triggered = true;
                    return await Task.FromResult(true);
                }, new Error("X", "Y"))
            .BuildAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        triggered.Should().BeFalse();
    }
    
    [Fact]
    public async Task AsyncChain_DropIfFail_StopsFurtherAsyncChecks()
    {
        // Arrange
        var triggered = false;
       
        // Act
        var result = await Result.Try()
            .CheckAsync(async () => await Task.FromResult(true), "Error1", "Desc1")
            .DropIfFail() 
            .CheckAsync(async () =>
            {
                triggered = true;
                return await Task.FromResult(true);
            }, "Error2", "Desc2")
            .BuildAsync();

        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Error1");
        triggered.Should().BeFalse();
    }
    
    [Fact]
    public void LargeChain_Performance_CompletesInReasonableTime()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
    
        var builder = Result.Try();
        
        // Act
        for (int i = 0; i < NumberOfPreformanceChecks; i++)
        {
            builder = builder.Check(false, new Error($"E{i}", $"D{i}"));
        }
        var result = builder.Build();
        stopwatch.Stop();
        
        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxReasonablePreformanceTimeMs);
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task LargeAsyncChain_Performance_CompletesInReasonableTime()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var builder = Result.Try().CheckAsync(async () => await Task.FromResult(false), new Error("E", "D"));
        
        // Act
        for (int i = 0; i < NumberOfPreformanceChecks; i++)
        {
            builder = builder.CheckAsync(async () => await Task.FromResult(false), new Error($"E{i}", $"D{i}"));
        }
        var result = await builder.BuildAsync();
        stopwatch.Stop();
        
        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxReasonablePreformanceTimeMs);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("message", "description")]
    [InlineData("m", "description")]
    [InlineData("d", "description")]
    public void CheckWithStringParams_ErrorConditionTrue_FailsResult(string msg, string desc)
    {
        // Arrange & Act
        var result = Result.Try()
            .Check(true, msg, desc)
            .Build();
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.First().Message.Should().Be(msg);
        result.Errors.First().Description.Should().Be(desc);
    }

    [Theory]
    [InlineData("message async", "description async")]
    [InlineData("m", "description")]
    [InlineData("d", "description")]
    public async Task CheckAsyncWithStringParams_ErrorConditionTrue_FailsResult(string msg, string desc)
    {
        // Arrange & Act
        var result = await Result.Try()
            .CheckAsync(async () => await Task.FromResult(true), msg, desc)
            .BuildAsync();
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.First().Message.Should().Be(msg);
        result.Errors.First().Description.Should().Be(desc);
    }
}