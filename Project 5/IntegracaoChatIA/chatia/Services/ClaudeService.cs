using Anthropic;
using Anthropic.Models.Messages;

namespace ChatIA.Services;

// Conversa com o Claude via SDK oficial. System = [PERSONA] + [BASE (cacheada)].
public class ClaudeService
{
    private readonly AnthropicClient _client;
    private readonly FaqStore _faq;
    private readonly string _fallbackBase;
    public string ModelId { get; }

    public ClaudeService(IConfiguration cfg, FaqStore faq, IWebHostEnvironment env)
    {
        var apiKey = cfg["ANTHROPIC_API_KEY"];
        _client = string.IsNullOrEmpty(apiKey)
            ? new AnthropicClient()                       // usa env ANTHROPIC_API_KEY
            : new AnthropicClient { ApiKey = apiKey };
        ModelId = cfg["CHAT_MODEL"] ?? "claude-haiku-4-5";
        _faq = faq;
        _fallbackBase = PromptConfig.LoadFallbackBase(env.ContentRootPath);
    }

    // Faz streaming chamando onText a cada pedaço de texto recebido.
    public async Task StreamAsync(IEnumerable<(string role, string content)> messages, Func<string, Task> onText, CancellationToken ct)
    {
        var faqText = await _faq.GetActiveFaqTextAsync();
        var baseText = string.IsNullOrWhiteSpace(faqText) ? _fallbackBase : faqText;

        var system = new List<TextBlockParam>
        {
            new() { Text = PromptConfig.Persona },
            new()
            {
                Text = "BASE DE CONHECIMENTO (perguntas e respostas):\n\n" + baseText,
                CacheControl = new CacheControlEphemeral(),   // cacheia PERSONA + BASE (prefixo)
            },
        };

        var msgs = messages.Select(m => new MessageParam
        {
            Role = m.role == "assistant" ? Role.Assistant : Role.User,
            Content = m.content,
        }).ToList();

        var parameters = new MessageCreateParams
        {
            Model = ModelId,
            MaxTokens = 2048,
            System = system,
            Messages = msgs,
            // sem Thinking: Haiku responde direto (texto/leitura), menor latência
        };

        await foreach (var ev in _client.Messages.CreateStreaming(parameters).WithCancellation(ct))
        {
            if (ev.TryPickContentBlockDelta(out var delta) && delta.Delta.TryPickText(out var text))
                await onText(text.Text);
        }
    }
}
