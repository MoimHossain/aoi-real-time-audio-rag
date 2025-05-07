
using System.Text.Json.Serialization;

namespace AzOpenAI.Rag.Rtc.Shared.Dtos
{
    public record SessionConfig(
        [property: JsonPropertyName("session")] SessionPayload SessionPayload,
        [property: JsonPropertyName("webRTCUri")] string WebRTCUri,
        [property: JsonPropertyName("modelDeploymentName")] string ModelDeploymentName);
}
