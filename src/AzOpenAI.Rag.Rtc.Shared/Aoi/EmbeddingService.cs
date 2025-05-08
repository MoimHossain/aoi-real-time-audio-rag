

using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;

namespace AzOpenAI.Rag.Rtc.Shared.Aoi
{
    public partial class EmbeddingService(
        AzureOpenAIConfiguration openAIConfig,
        AzureOpenAIClient openAIClient,
        ILogger<EmbeddingService> logger)
    {
        public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
            string text, CancellationToken cancellationToken)
        {
            logger.LogInformation("Generating embedding for text: {Text}", text);
            var embeddingClient = openAIClient.GetEmbeddingClient(openAIConfig.EmbeddingDeploymentId);
            var options = new OpenAI.Embeddings.EmbeddingGenerationOptions { };
            var embeddings = await embeddingClient.GenerateEmbeddingAsync(text, options, cancellationToken);
            var vectors = embeddings.Value.ToFloats();
            return vectors;
        }
    }
}
