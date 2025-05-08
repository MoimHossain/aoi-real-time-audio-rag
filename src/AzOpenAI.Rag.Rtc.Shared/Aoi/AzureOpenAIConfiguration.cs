using AzOpenAI.Rag.Rtc.Shared.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzOpenAI.Rag.Rtc.Shared.Aoi
{
    public class AzureOpenAIConfiguration : ConfigurationBase
    {
        public string Endpoint => GetRequiredAsString("AZURE_OPENAI_ENDPOINT", string.Empty);
        public string Key => GetRequiredAsString("OPEN_AI_API_KEY", string.Empty);
                
        public string EmbeddingDeploymentId => GetRequiredAsString("AZURE_OPENAI_EMBED_DEPLOYMENT_ID", string.Empty);
    }

    public class AzureSearchConfiguration : ConfigurationBase
    {
        public Uri Endpoint => new(GetRequiredAsString("AZURE_SEARCH_SERVICE_ENDPOINT", "https://microsoft.com"));
        public string Key => GetRequiredAsString("AZURE_SEARCH_SERVICE_API_KEY", string.Empty);
        public string IndexName => GetRequiredAsString("AZURE_SEARCH_SERVICE_INDEX_NAME", string.Empty);
    }
}
