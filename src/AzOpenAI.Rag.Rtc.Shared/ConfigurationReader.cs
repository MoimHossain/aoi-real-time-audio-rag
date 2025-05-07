
using AzOpenAI.Rag.Rtc.Shared.Dtos;

namespace AzOpenAI.Rag.Rtc.Shared
{
    public class ConfigurationReader
    {
        public const string SessionURI_KEY = "SESSION_URI";
        public const string WebRTCURI_KEY = "WEB_RTC_URI";
        public const string DeploymentName_KEY = "DEPLOYMENT_NAME";
        public const string OpenAI_API_KEY_KEY = "OPEN_AI_API_KEY";

        private string ReadEnvironmentVariable(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException($"Environment variable '{key}' is not set.");
            }
            return value;
        }

        public ConfigBundle GetConfigBundle()
        {
            var sessionUri = ReadEnvironmentVariable(SessionURI_KEY);
            var webRtcUri = ReadEnvironmentVariable(WebRTCURI_KEY);
            var deploymentName = ReadEnvironmentVariable(DeploymentName_KEY);
            var openAiApiKey = ReadEnvironmentVariable(OpenAI_API_KEY_KEY);
            return new ConfigBundle
            {
                SessionUri = sessionUri,
                WebRtcUri = webRtcUri,
                DeploymentName = deploymentName,
                OpenAiApiKey = openAiApiKey
            };
        }
    }
}
