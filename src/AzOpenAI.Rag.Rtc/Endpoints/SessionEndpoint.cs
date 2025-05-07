

using AzOpenAI.Rag.Rtc.Shared;
using AzOpenAI.Rag.Rtc.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AzOpenAI.Rag.Rtc.Endpoints
{
    public class SessionEndpoint
    {
        public async Task<SessionConfig> HandleAsync(
            [FromBody] SessionCreatePayload payload,
            [FromServices] ConfigurationReader configurationReader,
            [FromServices] IHttpClientFactory httpClientFactory,
            [FromServices] ILogger<SessionEndpoint> logger,            
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));
            ArgumentNullException.ThrowIfNull(payload.Voice, nameof(payload.Voice));

            var configBundle = configurationReader.GetConfigBundle();

            var httpClient = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, configBundle.SessionUri);
            request.Headers.Add("api-key", configBundle.OpenAiApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(new
            {
                model = configBundle.DeploymentName,
                voice = payload.Voice
            }), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var sessionPayload = await response.Content.ReadFromJsonAsync<SessionPayload>(cancellationToken: cancellationToken);

                return sessionPayload == null
                    ? throw new Exception("Session payload is null.")
                    : new SessionConfig(
                    SessionPayload: sessionPayload,
                    WebRTCUri: configBundle.WebRtcUri,
                    ModelDeploymentName: configBundle.DeploymentName);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Error occured {statusCode} {errorMessage}", response.StatusCode, errorMessage);
                throw new Exception($"Error: {response.StatusCode}, {errorMessage}");
            }
        }
    }
}
