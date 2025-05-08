using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AzOpenAI.Rag.Rtc.Shared.Dtos
{
    public class SearchResultPayloads
    {
        public record ContentRecord(string Title, string Content);

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum RetrievalMode
        {
            /// <summary>
            /// Text-only model, where only query will be used to retrieve the results
            /// </summary>
            Text = 0,

            /// <summary>
            /// Vector-only model, where only embeddings will be used to retrieve the results
            /// </summary>
            Vector,

            /// <summary>
            /// Text + Vector model, where both query and embeddings will be used to retrieve the results
            /// </summary>
            Hybrid,
        }

        public record RequestOverrides
        {
            [JsonPropertyName("semantic_ranker")]
            public bool SemanticRanker { get; set; } = false;

            [JsonPropertyName("retrieval_mode")]
            public RetrievalMode RetrievalMode { get; set; } = RetrievalMode.Vector; // available option: Text, Vector, Hybrid

            [JsonPropertyName("semantic_captions")]
            public bool? SemanticCaptions { get; set; }

            [JsonPropertyName("exclude_category")]
            public string? ExcludeCategory { get; set; }

            [JsonPropertyName("top")]
            public int? Top { get; set; } = 3;

            [JsonPropertyName("temperature")]
            public int? Temperature { get; set; }

            [JsonPropertyName("prompt_template")]
            public string? PromptTemplate { get; set; }

            [JsonPropertyName("prompt_template_prefix")]
            public string? PromptTemplatePrefix { get; set; }

            [JsonPropertyName("prompt_template_suffix")]
            public string? PromptTemplateSuffix { get; set; }

            [JsonPropertyName("suggest_followup_questions")]
            public bool? SuggestFollowupQuestions { get; set; } = true;

            [JsonPropertyName("use_gpt4v")]
            public bool? UseGPT4V { get; set; } = false;

            [JsonPropertyName("use_oid_security_filter")]
            public bool? UseOIDSecurityFilter { get; set; } = false;

            [JsonPropertyName("use_groups_security_filter")]
            public bool? UseGroupsSecurityFilter { get; set; } = false;

            [JsonPropertyName("vector_fields")]
            public bool? VectorFields { get; set; } = false;
        }
    }

    public record IndexConfig(string IndexName, string VectorFieldName, string ContentFieldName, string SourceDocumentFieldName);
}
