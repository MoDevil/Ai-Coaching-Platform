using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Auth.Validators;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Clients.Validators;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class ValidatorTests
{
    private readonly CreateClientRequestValidator _createClientValidator = new();
    private readonly RegisterCoachRequestValidator _registerCoachValidator = new();
    private readonly LoginCoachRequestValidator _loginCoachValidator = new();

    [Fact]
    public void CreateClientValidator_WhenRequiredFieldsOnly_ShouldBeValid()
    {
        var request = new CreateClientRequestDto(
            FirstName: "Khaled",
            LastName: "Mostafa"
        );

        var result = _createClientValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateClientValidator_WhenFirstNameEmpty_ShouldBeInvalid()
    {
        var request = new CreateClientRequestDto(
            FirstName: "",
            LastName: "Mostafa"
        );

        var result = _createClientValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public void CreateClientValidator_WhenLastNameEmpty_ShouldBeInvalid()
    {
        var request = new CreateClientRequestDto(
            FirstName: "Khaled",
            LastName: ""
        );

        var result = _createClientValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public void CreateClientValidator_WhenEmailInvalid_ShouldBeInvalid()
    {
        var request = new CreateClientRequestDto(
            FirstName: "Khaled",
            LastName: "Mostafa",
            Email: "not-an-email"
        );

        var result = _createClientValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void CreateClientValidator_WhenTargetTimelineWeeksZero_ShouldBeInvalid()
    {
        var request = new CreateClientRequestDto(
            FirstName: "Khaled",
            LastName: "Mostafa",
            Goal: new ClientGoalDto("Hypertrophy", 0)
        );

        var result = _createClientValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Goal.TargetTimelineWeeks");
    }

    [Fact]
    public void RegisterCoachValidator_WhenValid_ShouldBeValid()
    {
        var request = new RegisterCoachRequestDto("Captain Hossam", "hossam@coach.eg", "StrongP@ssw0rd");

        var result = _registerCoachValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RegisterCoachValidator_WhenPasswordTooShort_ShouldBeInvalid()
    {
        var request = new RegisterCoachRequestDto("Captain Hossam", "hossam@coach.eg", "12345");

        var result = _registerCoachValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void LoginCoachValidator_WhenValid_ShouldBeValid()
    {
        var request = new LoginCoachRequestDto("hossam@coach.eg", "StrongP@ssw0rd");

        var result = _loginCoachValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
