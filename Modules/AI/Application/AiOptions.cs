namespace ERPPlatform.Modules.AI.Application;

/// <summary>
/// Configuration for the AI assistant provider. Bound from the "AI" section
/// in appsettings.json. The provider is OpenAI-compatible (OpenAI, Azure
/// OpenAI, or any /chat/completions endpoint such as Ollama or a local model).
/// </summary>
public class AiOptions
{
    /// <summary>
    /// When false the assistant returns deterministic, offline answers. Set to
    /// true once an ApiKey is configured.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>Provider label, e.g. "OpenAI", "AzureOpenAI", "Ollama".</summary>
    public string Provider { get; set; } = "OpenAI";

    /// <summary>Bearer token sent in the Authorization header. Leave empty to disable live calls.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Full /chat/completions URL.</summary>
    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";

    /// <summary>Model identifier, e.g. "gpt-4o-mini".</summary>
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>Maximum tokens to generate per response.</summary>
    public int MaxTokens { get; set; } = 800;

    /// <summary>Sampling temperature (0 = deterministic, 1 = creative).</summary>
    public double Temperature { get; set; } = 0.3;
}
