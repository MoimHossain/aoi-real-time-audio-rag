using AzOpenAI.Rag.Rtc.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AzOpenAI.Rag.Rtc.Endpoints
{
    public class SearchEndpoint
    {
        public async Task<List<SearchResult>> HandleAsync(
            [FromBody] SearchPayload payload,            
            [FromServices] ILogger<SearchEndpoint> logger,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));
            ArgumentNullException.ThrowIfNull(payload.SearchKey, nameof(payload.SearchKey));
        

            logger.LogInformation("Search key: {SearchKey}", payload.SearchKey);
            await Task.Delay(1000, cancellationToken); // Simulate async work

            return new List<SearchResult>()
            {
                new SearchResult("The hotline number is 09-110-256-356.")                
            };
        }
    }
}
