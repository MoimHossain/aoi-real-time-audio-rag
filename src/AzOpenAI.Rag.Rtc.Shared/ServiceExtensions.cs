

using AzOpenAI.Rag.Rtc.Shared.Aoi;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzOpenAI.Rag.Rtc.Shared
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMandetoryServicesForWebAPI(
            this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            services.AddHttpContextAccessor();
            return services;
        }
        public static IServiceCollection AddMandetoryServices(
            this IServiceCollection services, ConfigurationManager configurationManager)
        {
            services.AddHttpClient();
            services.AddLogging(logging =>
            {
                logging.AddConfiguration(configurationManager.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            });
            services.AddSingleton(services =>
            {
                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    AllowTrailingCommas = true,
                    PropertyNameCaseInsensitive = true
                };
                jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                return jsonSerializerOptions;
            });
            services.AddSingleton<ConfigurationReader>();
            return services;
        }

        public static IServiceCollection AddAzureOpenAIServices(this IServiceCollection services)
        {
            services.AddSingleton<AzureOpenAIConfiguration>();
            services.AddSingleton<AzureSearchConfiguration>();
            
            services.AddSingleton(builder =>
            {
                var openAIConfig = builder.GetRequiredService<AzureOpenAIConfiguration>();
                return new AzureOpenAIClient(
                    new Uri(openAIConfig.Endpoint),
                    new AzureKeyCredential(openAIConfig.Key));
            });
                        
            services.AddSingleton(serviceProvider =>
            {
                var searchConfiguration = serviceProvider.GetRequiredService<AzureSearchConfiguration>();
                return new SearchClient(
                    searchConfiguration.Endpoint,
                    searchConfiguration.IndexName,
                    new AzureKeyCredential(searchConfiguration.Key));
            });

            services.AddSingleton<EmbeddingService>();
            services.AddSingleton<SearchService>();

            return services;
        }
    }
}
