namespace AntiAway.Models;

public sealed record ActivityInterval(int Seconds, string Title, string ShortTitle)
{
    public static IReadOnlyList<ActivityInterval> All { get; } =
    [
        new(30, "Every 30 seconds", "30 sec"),
        new(60, "Every minute", "1 min"),
        new(120, "Every 2 minutes", "2 min"),
        new(240, "Every 4 minutes", "4 min")
    ];

    public static ActivityInterval FromSeconds(int seconds) =>
        All.FirstOrDefault(interval => interval.Seconds == seconds) ?? All[1];
}

