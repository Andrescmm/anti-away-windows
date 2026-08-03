namespace AntiAway.Models;

public sealed class AppSettings
{
    public bool IsEnabled { get; set; }

    public int IntervalSeconds { get; set; } = 60;

    public bool KeepComputerAwake { get; set; } = true;

    public bool HasCompletedOnboarding { get; set; }
}

