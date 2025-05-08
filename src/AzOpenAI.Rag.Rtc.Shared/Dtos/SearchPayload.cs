
using System.Text.Json.Serialization;

namespace AzOpenAI.Rag.Rtc.Shared.Dtos
{
    public record SearchPayload([property: JsonPropertyName("searchKey")] string SearchKey);
}
