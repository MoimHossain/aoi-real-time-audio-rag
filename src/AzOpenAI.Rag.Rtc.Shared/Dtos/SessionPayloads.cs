
using System.Text.Json.Serialization;

namespace AzOpenAI.Rag.Rtc.Shared.Dtos
{
    public record SessionPayload(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("object")] string Object,
        [property: JsonPropertyName("expires_at")] int? ExpiresAt,        
        [property: JsonPropertyName("turn_detection")] TurnDetection TurnDetection,
        [property: JsonPropertyName("input_audio_format")] string InputAudioFormat,        
        [property: JsonPropertyName("client_secret")] ClientSecret ClientSecret,        
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("modalities")] IReadOnlyList<string> Modalities,
        [property: JsonPropertyName("instructions")] string Instructions,
        [property: JsonPropertyName("voice")] string Voice,
        [property: JsonPropertyName("output_audio_format")] string OutputAudioFormat,
        [property: JsonPropertyName("tool_choice")] string ToolChoice,
        [property: JsonPropertyName("temperature")] double? Temperature,
        [property: JsonPropertyName("max_response_output_tokens")] string MaxResponseOutputTokens
    );

    public record ClientSecret(
        [property: JsonPropertyName("value")] string Value,
        [property: JsonPropertyName("expires_at")] int? ExpiresAt
    );

    public record TurnDetection(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("threshold")] double? Threshold,
        [property: JsonPropertyName("prefix_padding_ms")] int? PrefixPaddingMs,
        [property: JsonPropertyName("silence_duration_ms")] int? SilenceDurationMs,
        [property: JsonPropertyName("create_response")] bool? CreateResponse,
        [property: JsonPropertyName("interrupt_response")] bool? InterruptResponse
    );
}
