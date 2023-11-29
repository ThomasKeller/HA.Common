namespace HA.Common.Mqtt;

public interface IMqttPublisher
{
    string? ClientId { get; }

    Task Disconnect();

    Task<bool> Publish(IDictionary<string, string> topicsAndpPayloads);

    Task<bool> Publish(string topic, string payload);

    Task<bool> Publish(Measurement measurement);
}