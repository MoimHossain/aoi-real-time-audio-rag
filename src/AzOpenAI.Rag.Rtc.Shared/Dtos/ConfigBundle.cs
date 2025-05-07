namespace AzOpenAI.Rag.Rtc.Shared.Dtos
{
    public class ConfigBundle
    {
        public string SessionUri { get; internal set; } = string.Empty;
        public string WebRtcUri { get; internal set; } = string.Empty;
        public string DeploymentName { get; internal set; } = string.Empty;
        public string OpenAiApiKey { get; internal set; } = string.Empty;
    }
}
