using AcademicManager.Application.Common.Errors;
using FluentAssertions;
using Xunit;

namespace AcademicManager.Tests.Unit.Common.Errors;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Success_WithValue_ShouldReturnSuccessfulResultWithValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_ShouldReturnFailedResult()
    {
        var error = Error.NotFound("NOT_FOUND", "Entity not found");
        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void Failure_WithValue_ShouldReturnFailedResultWithDefaultValue()
    {
        var error = Error.Validation("INVALID", "Invalid input");
        var result = Result.Failure<int>(error);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
}
