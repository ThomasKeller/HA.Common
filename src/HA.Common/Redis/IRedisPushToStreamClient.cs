namespace HA.Common.Redis
{
    public interface IRedisPushToStreamClient
    {
        bool PushToStream(Measurement measurement);
    }
}