using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AiCoachOs.Api.Middleware;
using AiCoachOs.Application.Common.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Infrastructure;

public class ExceptionHandlingMiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExceptionHandlingMiddlewareTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task Middleware_ValidationException_Returns400WithDetails()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var errors = new Dictionary<string, string[]>
        {
            { "ClientId", new[] { "ClientId is required." } }
        };
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ValidationException(errors),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await JsonDocument.ParseAsync(context.Response.Body);
        body.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        body.RootElement.GetProperty("title").GetString().Should().Be("Validation Error");
    }

    [Fact]
    public async Task Middleware_KeyNotFoundException_Returns404NotFound()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new KeyNotFoundException("Resource not found."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(404);
        problem.Title.Should().Be("Not Found");
        problem.Detail.Should().Be("Resource not found.");
    }

    [Fact]
    public async Task Middleware_UnauthorizedAccessException_Returns403Forbidden()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new UnauthorizedAccessException("Forbidden access."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(403);
    }

    [Fact]
    public async Task Middleware_UnhandledInternalException_MasksMessageIn500Response()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new Exception("Sensitive internal database connection failed: Secret123"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
        problem.Title.Should().Be("Internal Server Error");
        problem.Detail.Should().Be("An unexpected error occurred while processing your request.");
        problem.Detail.Should().NotContain("Secret123");
    }
}
