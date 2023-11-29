namespace HA.Common;

public interface IObserverProcessor
{
    void ProcessMeasurement(Measurement measurement);
}