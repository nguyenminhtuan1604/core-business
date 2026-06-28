using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace CoreBusinessService.Services;

public class AnalyticsMqttService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AnalyticsMqttService> _logger;

    public AnalyticsMqttService(
        IConfiguration configuration,
        ILogger<AnalyticsMqttService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Sent, string? Error)> PublishAlertAsync(
        object payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var host = _configuration["ANALYTICS_MQTT_HOST"] ?? "26.109.160.213";
            var port = int.Parse(_configuration["ANALYTICS_MQTT_PORT"] ?? "1883");
            var topic = _configuration["ANALYTICS_MQTT_TOPIC"] ?? "smart-campus/events/alert";

            var jsonPayload = JsonSerializer.Serialize(payload);

            var factory = new MqttFactory();
            using var client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithClientId($"core-business-{Guid.NewGuid()}")
                .Build();

            await client.ConnectAsync(options, cancellationToken);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(jsonPayload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await client.PublishAsync(message, cancellationToken);
            await client.DisconnectAsync();

            _logger.LogInformation(
                "Published alert to Analytics MQTT. Host={Host}, Port={Port}, Topic={Topic}, Payload={Payload}",
                host,
                port,
                topic,
                jsonPayload);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish alert to Analytics MQTT");
            return (false, ex.Message);
        }
    }
}
