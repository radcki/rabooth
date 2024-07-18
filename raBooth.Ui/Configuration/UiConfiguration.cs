namespace raBooth.Ui.Configuration;

public class UiConfiguration
{
    public static string SectionName = nameof(UiConfiguration);

    public string Culture { get; init; }
    public TimeSpan EnableSleepModeTime { get; init; } = TimeSpan.FromSeconds(5);
}