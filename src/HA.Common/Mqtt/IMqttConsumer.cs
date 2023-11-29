namespace HA.Common.Mqtt;

public interface IMqttConsumer
{
    string? ClientId { get; }

    Task<bool> Subscribe(string topic);
}