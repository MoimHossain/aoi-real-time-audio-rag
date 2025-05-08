namespace AzOpenAI.Rag.Rtc.Shared.Abstracts
{
    public abstract class ConfigurationBase
    {
        protected string GetRequiredAsString(string name, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        protected bool GetRequiredAsBool(string name, bool defaultValue)
        {
            var value = ReadRequiredEnvironmentKey(name);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : bool.Parse(value);
        }

        protected string? ReadRequiredEnvironmentKey(string key, bool throwIfNotConfigured = true)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value) && throwIfNotConfigured)
            {
                throw new InvalidOperationException($"Environment variable {key} is not set");
            }
            return value;
        }
    }
}
