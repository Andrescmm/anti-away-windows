using System.ComponentModel;
using System.Runtime.CompilerServices;
using AntiAway.Models;
using AntiAway.Services;
using Microsoft.UI.Dispatching;

namespace AntiAway.ViewModels;

public sealed class AppViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ActivityService _activityService;
    private readonly PowerService _powerService;
    private readonly SettingsService _settingsService;
    private readonly StartupService _startupService;
    private readonly AppSettings _settings;
    private readonly DispatcherQueueTimer _activityTimer;
    private readonly DispatcherQueueTimer _clockTimer;

    private bool _isEnabled;
    private bool _launchAtLogin;
    private ActivityInterval _interval;
    private DateTimeOffset? _lastActivityAt;
    private string? _errorMessage;

    public AppViewModel(DispatcherQueue dispatcherQueue)
    {
        _activityService = new ActivityService();
        _powerService = new PowerService();
        _settingsService = new SettingsService();
        _startupService = new StartupService();
        _settings = _settingsService.Load();

        _interval = ActivityInterval.FromSeconds(_settings.IntervalSeconds);
        _launchAtLogin = _startupService.IsEnabled();

        _activityTimer = dispatcherQueue.CreateTimer();
        _activityTimer.IsRepeating = true;
        _activityTimer.Tick += ActivityTimerTick;

        _clockTimer = dispatcherQueue.CreateTimer();
        _clockTimer.Interval = TimeSpan.FromSeconds(1);
        _clockTimer.IsRepeating = true;
        _clockTimer.Tick += ClockTimerTick;
        _clockTimer.Start();

        if (_settings.IsEnabled)
        {
            SetEnabled(true);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<ActivityInterval> Intervals => ActivityInterval.All;

    public bool IsEnabled
    {
        get => _isEnabled;
        private set
        {
            if (SetField(ref _isEnabled, value))
            {
                OnPropertyChanged(nameof(StatusTitle));
                OnPropertyChanged(nameof(StatusDetail));
            }
        }
    }

    public bool LaunchAtLogin
    {
        get => _launchAtLogin;
        private set => SetField(ref _launchAtLogin, value);
    }

    public bool KeepComputerAwake
    {
        get => _settings.KeepComputerAwake;
        private set
        {
            if (_settings.KeepComputerAwake == value)
            {
                return;
            }

            _settings.KeepComputerAwake = value;
            SaveSettings();
            OnPropertyChanged();

            if (IsEnabled)
            {
                TryUpdateKeepAwake();
            }
        }
    }

    public ActivityInterval Interval
    {
        get => _interval;
        private set
        {
            if (!SetField(ref _interval, value))
            {
                return;
            }

            _settings.IntervalSeconds = value.Seconds;
            SaveSettings();
            OnPropertyChanged(nameof(StatusDetail));

            if (IsEnabled)
            {
                ScheduleActivityTimer();
            }
        }
    }

    public DateTimeOffset? LastActivityAt
    {
        get => _lastActivityAt;
        private set
        {
            if (SetField(ref _lastActivityAt, value))
            {
                OnPropertyChanged(nameof(StatusDetail));
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetField(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool IsOnboardingVisible => !_settings.HasCompletedOnboarding;

    public string CurrentTime => DateTimeOffset.Now.ToString("t");

    public string StatusTitle => IsEnabled ? "Staying active" : "AntiAway is off";

    public string StatusDetail
    {
        get
        {
            if (!IsEnabled)
            {
                return "Ready when you are";
            }

            if (LastActivityAt is null)
            {
                return $"Signal every {Interval.ShortTitle}";
            }

            TimeSpan elapsed = DateTimeOffset.Now - LastActivityAt.Value;
            return elapsed < TimeSpan.FromMinutes(1)
                ? "Last signal just now"
                : $"Last signal at {LastActivityAt.Value:t}";
        }
    }

    public void SetEnabled(bool enabled)
    {
        ErrorMessage = null;

        if (!enabled)
        {
            _activityTimer.Stop();
            IsEnabled = false;
            _settings.IsEnabled = false;
            SaveSettings();
            TryReleaseKeepAwake();
            return;
        }

        try
        {
            _activityService.PostActivity();
            LastActivityAt = DateTimeOffset.Now;
            IsEnabled = true;
            _settings.IsEnabled = true;
            SaveSettings();
            TryUpdateKeepAwake();
            ScheduleActivityTimer();
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            IsEnabled = false;
            _settings.IsEnabled = false;
            SaveSettings();
            ErrorMessage = exception.Message;
        }
    }

    public void SetInterval(ActivityInterval interval)
    {
        Interval = interval;
    }

    public void SetLaunchAtLogin(bool enabled)
    {
        ErrorMessage = null;

        try
        {
            _startupService.SetEnabled(enabled);
            LaunchAtLogin = _startupService.IsEnabled();
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException or InvalidOperationException)
        {
            LaunchAtLogin = _startupService.IsEnabled();
            ErrorMessage = $"Could not update launch at login: {exception.Message}";
        }
    }

    public void SetKeepComputerAwake(bool enabled)
    {
        KeepComputerAwake = enabled;
    }

    public void CompleteOnboarding()
    {
        if (_settings.HasCompletedOnboarding)
        {
            return;
        }

        _settings.HasCompletedOnboarding = true;
        SaveSettings();
        OnPropertyChanged(nameof(IsOnboardingVisible));
    }

    public void ShowOnboarding()
    {
        if (!_settings.HasCompletedOnboarding)
        {
            return;
        }

        _settings.HasCompletedOnboarding = false;
        SaveSettings();
        OnPropertyChanged(nameof(IsOnboardingVisible));
    }

    public void DismissError()
    {
        ErrorMessage = null;
    }

    public void Dispose()
    {
        _activityTimer.Stop();
        _clockTimer.Stop();
        TryReleaseKeepAwake();
        _powerService.Dispose();
    }

    private void ActivityTimerTick(DispatcherQueueTimer sender, object args)
    {
        try
        {
            _activityService.PostActivity();
            LastActivityAt = DateTimeOffset.Now;
        }
        catch (Win32Exception exception)
        {
            _activityTimer.Stop();
            IsEnabled = false;
            _settings.IsEnabled = false;
            SaveSettings();
            TryReleaseKeepAwake();
            ErrorMessage = exception.Message;
        }
    }

    private void ClockTimerTick(DispatcherQueueTimer sender, object args)
    {
        OnPropertyChanged(nameof(CurrentTime));
        if (IsEnabled)
        {
            OnPropertyChanged(nameof(StatusDetail));
        }
    }

    private void ScheduleActivityTimer()
    {
        _activityTimer.Stop();
        _activityTimer.Interval = TimeSpan.FromSeconds(Interval.Seconds);
        _activityTimer.Start();
    }

    private void TryUpdateKeepAwake()
    {
        try
        {
            _powerService.SetKeepAwake(KeepComputerAwake);
        }
        catch (Win32Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    private void TryReleaseKeepAwake()
    {
        try
        {
            _powerService.SetKeepAwake(false);
        }
        catch (Win32Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    private void SaveSettings()
    {
        try
        {
            _settingsService.Save(_settings);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ErrorMessage = $"Settings could not be saved: {exception.Message}";
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
