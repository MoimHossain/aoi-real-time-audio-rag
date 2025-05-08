using System.Text.Json.Serialization;

namespace AzOpenAI.Rag.Rtc.Shared.Dtos
{
    public record SearchResult([property: JsonPropertyName("content")] string Content);
}
