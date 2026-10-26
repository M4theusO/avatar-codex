using System.Text.Json;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace AvatarCodex.IntegrationTests.Api;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    [Fact]
    public async Task UnexpectedException_Returns500WithoutInternalDetails()
    {
        var (status, body) = await HandleAsync(new InvalidOperationException("connection string secreta"));

        Assert.Equal(500, status);
        Assert.DoesNotContain("secreta", body);
        Assert.DoesNotContain("InvalidOperationException", body);
    }

    [Theory]
    [InlineData(typeof(ConflictException), 409)]
    [InlineData(typeof(BusinessRuleException), 422)]
    [InlineData(typeof(NotFoundException), 404)]
    public async Task DomainExceptions_MapToStatusCodesWithTheirMessage(Type exceptionType, int expectedStatus)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Mensagem amigável")!;

        var (status, body) = await HandleAsync(exception);

        Assert.Equal(expectedStatus, status);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(expectedStatus, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Mensagem amigável", json.RootElement.GetProperty("message").GetString());
        Assert.Equal(0, json.RootElement.GetProperty("errors").GetArrayLength());
    }

    private async Task<(int Status, string Body)> HandleAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        context.Response.Body.Position = 0;
        return (context.Response.StatusCode, await new StreamReader(context.Response.Body).ReadToEndAsync());
    }
}
