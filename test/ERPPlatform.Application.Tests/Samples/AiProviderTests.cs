using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ERPPlatform.Modules.AI.Application;
using Shouldly;
using Xunit;

namespace ERPPlatform.Samples;

/// <summary>
/// Unit tests for <see cref="OpenAiCompatibleAiProvider"/> using a fake
/// HttpMessageHandler so no network or API key is required.
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

    public HttpRequestMessage? CapturedRequest { get; private set; }

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        => _respond = respond;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CapturedRequest = request;
        return Task.FromResult(_respond(request));
    }
}

public class FakeHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;
    public FakeHttpClientFactory(HttpClient client) => _client = client;
    public HttpClient CreateClient(string name) => _client;
}

public class AiProviderTests
{
    private static OpenAiCompatibleAiProvider Build(FakeHttpMessageHandler handler)
        => new(new FakeHttpClientFactory(new HttpClient(handler)));

    [Fact]
    public async Task CompleteAsync_Parses_Content_And_Sets_Bearer_Header()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                choices = new[] { new { message = new { role = "assistant", content = "Hello from model" } } }
            }))
        });

        var provider = Build(handler);
        var options = new AiOptions { ApiKey = "sk-test", Model = "gpt-4o-mini" };

        var result = await provider.CompleteAsync(
            new[] { new AiChatMessageDto { Role = "user", Content = "hi" } }, options);

        result.ShouldBe("Hello from model");
        handler.CapturedRequest!.Headers.Authorization!.Scheme.ShouldBe("Bearer");
        handler.CapturedRequest!.Headers.Authorization!.Parameter.ShouldBe("sk-test");
    }

    [Fact]
    public async Task CompleteAsync_Throws_On_NonSuccess_Status()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("boom")
        });

        var provider = Build(handler);

        await Should.ThrowAsync<HttpRequestException>(async () =>
            await provider.CompleteAsync(
                new[] { new AiChatMessageDto { Role = "user", Content = "x" } },
                new AiOptions { ApiKey = "k" }));
    }
}
