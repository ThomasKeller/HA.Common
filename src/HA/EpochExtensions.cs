namespace HA;

public static class EpochExtensions
{
    private static readonly DateTime s_Origin = new DateTime(new DateTime(1970, 1, 1).Ticks, DateTimeKind.Utc);

    public static long ToEpoch(this DateTime time)
    {
        var t = time - s_Origin;
        return t.Ticks * 100; //1 tick = 100 nano sec
    }

    public static DateTime FromEpoch(this long duration)
    {
        return s_Origin.AddTicks(duration / 100); //1 tick = 100 nano sec
    }
}