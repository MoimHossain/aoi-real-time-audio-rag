

using System.Text.Json.Serialization;

namespace AzOpenAI.Rag.Rtc.Shared.Dtos
{
    public record SessionCreatePayload([property: JsonPropertyName("voice")] string Voice);
}
