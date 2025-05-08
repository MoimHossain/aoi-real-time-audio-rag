

using AzOpenAI.Rag.Rtc.Shared.Dtos;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using static AzOpenAI.Rag.Rtc.Shared.Dtos.SearchResultPayloads;

namespace AzOpenAI.Rag.Rtc.Shared.Aoi
{
    public class SearchService(
        SearchClient searchClient,
        ILogger<SearchService> logger)
    {
        public async Task<ContentRecord[]> SearchAsync(
            IndexConfig indexConfig, ReadOnlyMemory<float> queryEmbedding, CancellationToken cancellationToken)
        {
            return await QueryDocumentsAsync(
                indexConfig,
                query: null,
                embedding: queryEmbedding,
                overrides: new RequestOverrides
                {
                    Top = 3,
                    SemanticRanker = false,
                    SemanticCaptions = false,
                    RetrievalMode = RetrievalMode.Text
                },
                cancellationToken: cancellationToken);
        }


        private async Task<ContentRecord[]> QueryDocumentsAsync(
            IndexConfig indexConfig,
            string? query = null,
            ReadOnlyMemory<float>? embedding = null,
            RequestOverrides? overrides = null,
            CancellationToken cancellationToken = default)
        {
            if (query is null && embedding is null)
            {
                throw new ArgumentException("Either query or embedding must be provided");
            }

            var documentContents = string.Empty;
            var top = overrides?.Top ?? 3;
            var exclude_category = overrides?.ExcludeCategory;
            var filter = exclude_category == null ? string.Empty : $"category ne '{exclude_category}'";
            var useSemanticRanker = overrides?.SemanticRanker ?? false;
            var useSemanticCaptions = overrides?.SemanticCaptions ?? false;

            SearchOptions searchOptions = useSemanticRanker
                ? new SearchOptions
                {
                    Filter = filter,
                    QueryType = SearchQueryType.Semantic,
                    SemanticSearch = new()
                    {
                        SemanticConfigurationName = "default",
                        QueryCaption = new(useSemanticCaptions
                            ? QueryCaptionType.Extractive
                            : QueryCaptionType.None),
                    },
                    // TODO: Find if these options are assignable
                    //QueryLanguage = "en-us",
                    //QuerySpeller = "lexicon",
                    Size = top,
                }
                : new SearchOptions
                {
                    Filter = filter,
                    Size = top,
                };

            if (embedding != null /*&& overrides?.RetrievalMode != RetrievalMode.Text*/)
            {
                var k = useSemanticRanker ? 50 : top;
                var vectorQuery = new VectorizedQuery(embedding.Value)
                {
                    // if semantic ranker is enabled, we need to set the rank to a large number to get more
                    // candidates for semantic reranking
                    KNearestNeighborsCount = useSemanticRanker ? 50 : top,
                };
                vectorQuery.Fields.Add(indexConfig.VectorFieldName);
                searchOptions.VectorSearch = new();
                searchOptions.VectorSearch.Queries.Add(vectorQuery);
            }

            var searchResultResponse = await searchClient.SearchAsync<SearchDocument>(
                query, searchOptions, cancellationToken);
            if (searchResultResponse.Value is null)
            {
                throw new InvalidOperationException("fail to get search result");
            }

            SearchResults<SearchDocument> searchResult = searchResultResponse.Value;

            var sb = new List<ContentRecord>();
            foreach (var doc in searchResult.GetResults())
            {
                doc.Document.TryGetValue(indexConfig.SourceDocumentFieldName, out var sourcePageValue);
                string? contentValue;
                try
                {
                    if (useSemanticCaptions)
                    {
                        var docs = doc.SemanticSearch.Captions.Select(c => c.Text);
                        contentValue = string.Join(" . ", docs);
                    }
                    else
                    {
                        doc.Document.TryGetValue(indexConfig.ContentFieldName, out var value);
                        contentValue = (string)value;
                    }
                }
                catch (ArgumentNullException ex)
                {
                    contentValue = null;
                    logger.LogError(ex, "Error while getting content from search result");
                }

                if (sourcePageValue is string sourcePage && contentValue is string content)
                {
                    content = content.Replace('\r', ' ').Replace('\n', ' ');
                    sb.Add(new ContentRecord(sourcePage, content));
                }
            }
            return [.. sb];
        }
    }
}
