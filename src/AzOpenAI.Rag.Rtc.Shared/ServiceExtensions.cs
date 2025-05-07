

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
    }
}
