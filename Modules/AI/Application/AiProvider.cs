using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ERPPlatform.Modules.AI.Application;

/// <summary>
/// Abstraction over an LLM chat provider. Implementations turn a list of
/// chat messages into the provider's request and extract the reply text.
/// </summary>
public interface IAiProvider
{
    Task<string> CompleteAsync(
        IReadOnlyList<AiChatMessageDto> messages,
        AiOptions options,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// OpenAI-compatible chat completion provider (OpenAI, Azure OpenAI, Ollama,
/// or any endpoint that accepts the standard /chat/completions shape).
/// </summary>
public class OpenAiCompatibleAiProvider : IAiProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenAiCompatibleAiProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> CompleteAsync(
        IReadOnlyList<AiChatMessageDto> messages,
        AiOptions options,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("AI");

        var payload = new
        {
            model = options.Model,
            messages = messages
                .Where(m => !string.IsNullOrWhiteSpace(m.Content))
                .Select(m => new { role = m.Role, content = m.Content })
                .ToArray(),
            max_tokens = options.MaxTokens,
            temperature = options.Temperature
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, options.Endpoint)
        {
            Content = JsonContent.Create(payload)
        };

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
        }

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"AI provider returned {(int)response.StatusCode}: {error}");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? string.Empty;
    }
}
