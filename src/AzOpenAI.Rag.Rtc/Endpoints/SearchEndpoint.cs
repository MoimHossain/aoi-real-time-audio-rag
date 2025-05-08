

using AzOpenAI.Rag.Rtc.Shared.Aoi;
using AzOpenAI.Rag.Rtc.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AzOpenAI.Rag.Rtc.Endpoints
{
    public class SearchEndpoint
    {
        public async Task<List<SearchResult>> HandleAsync(
            [FromBody] SearchPayload payload,
            [FromServices] ILogger<SearchEndpoint> logger,
            [FromServices] SearchService searchService,
            [FromServices] EmbeddingService embeddingService,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));
            ArgumentNullException.ThrowIfNull(payload.SearchKey, nameof(payload.SearchKey));

            logger.LogInformation("Search key: {SearchKey}", payload.SearchKey);
            List<SearchResult> searchResut = [];

            try
            {
                var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(payload.SearchKey, cancellationToken);
                var indexConfig = new IndexConfig("mh-demo-index", "embedding", "content", "sourcepage");

                var contentRecords = await searchService.SearchAsync(indexConfig, queryEmbedding, cancellationToken);
                if(contentRecords != null && contentRecords.Length > 0)
                {
                    foreach (var record in contentRecords)
                    {
                        searchResut.Add(new SearchResult(record.Content, record.Title));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while searching for {SearchKey}", payload.SearchKey);
            }

            return searchResut;
        }
    }
}
