using System.Text.Json;
using EnterpriseEmployeeManagementAPI.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnterpriseEmployeeManagementAPI.Tests;

public sealed class ExceptionMiddlewareTests
{
    [Theory]
    [InlineData(typeof(KeyNotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status409Conflict)]
    [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest)]
    [InlineData(typeof(Exception), StatusCodes.Status500InternalServerError)]
    public async Task InvokeAsyncMapsExceptionsToProblemDetails(
        Type exceptionType,
        int expectedStatusCode)
    {
        var exception = (Exception)Activator.CreateInstance(
            exceptionType,
            "Failure message")!;
        var middleware = new ExceptionMiddleware(
            _ => throw exception,
            NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(expectedStatusCode);
        context.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(expectedStatusCode);
        problem.Extensions.Should().ContainKey("traceId");
    }

    [Fact]
    public async Task InvokeAsyncRethrowsRequestCancellation()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var middleware = new ExceptionMiddleware(
            _ => throw new OperationCanceledException(source.Token),
            NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.RequestAborted = source.Token;

        Func<Task> act = () => middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
