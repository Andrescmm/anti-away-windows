using System.ComponentModel;
using System.Runtime.InteropServices;
using AntiAway.Models;
using AntiAway.Services;
using AntiAway.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

namespace AntiAway;

public sealed partial class MainWindow : Window, IDisposable
{
    private const int PanelWidth = 400;
    private const int PanelHeight = 590;
    private const int GwlExStyle = -20;
    private const nint WsExToolWindow = 0x00000080;
    private const nint WsExAppWindow = 0x00040000;

    private readonly SolidColorBrush _accentBrush = new(Windows.UI.Color.FromArgb(255, 199, 255, 51));
    private readonly SolidColorBrush _inactiveBrush = new(Windows.UI.Color.FromArgb(45, 255, 255, 255));
    private readonly SolidColorBrush _inactiveForegroundBrush = new(Windows.UI.Color.FromArgb(184, 255, 255, 255));
    private TrayIconService? _trayIconService;
    private AppWindow? _appWindow;
    private SettingsWindow? _settingsWindow;
    private nint _windowHandle;
    private bool _controlsReady;
    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();

        ViewModel = new AppViewModel(DispatcherQueue);
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        IntervalComboBox.ItemsSource = ViewModel.Intervals;
        IntervalComboBox.SelectedItem = ViewModel.Interval;
        StayActiveToggle.IsOn = ViewModel.IsEnabled;
        LaunchAtLoginToggle.IsOn = ViewModel.LaunchAtLogin;
        _controlsReady = true;

        SyncViewFromState();
        SyncOnboarding();
    }

    public AppViewModel ViewModel { get; }

    public void InitializeAfterActivation()
    {
        _windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(_windowHandle);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        _appWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "AntiAway.ico"));
        _appWindow.Closing += AppWindow_Closing;

        if (_appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.SetBorderAndTitleBar(false, false);
        }

        nint extendedStyle = GetWindowLongPtr(_windowHandle, GwlExStyle);
        extendedStyle = (extendedStyle | WsExToolWindow) & ~WsExAppWindow;
        _ = SetWindowLongPtr(_windowHandle, GwlExStyle, extendedStyle);

        SystemBackdrop = new DesktopAcrylicBackdrop();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(DragRegion);

        _trayIconService = new TrayIconService(
            _windowHandle,
            Path.Combine(AppContext.BaseDirectory, "Assets", "AntiAway.ico"));
        _trayIconService.Activated += TrayIconService_Activated;
        _trayIconService.UpdateState(ViewModel.IsEnabled);

        PositionNearSystemTray();
    }

    public void ShowPanel()
    {
        PositionNearSystemTray();
        _appWindow?.Show();
        Activate();
    }

    public void HidePanel()
    {
        _appWindow?.Hide();
    }

    public void Dispose()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        ViewModel.Dispose();

        if (_trayIconService is not null)
        {
            _trayIconService.Activated -= TrayIconService_Activated;
            _trayIconService.Dispose();
            _trayIconService = null;
        }

        _settingsWindow?.CloseForAppExit();
        _settingsWindow = null;
        Close();
    }

    private void PositionNearSystemTray()
    {
        if (_appWindow is null)
        {
            return;
        }

        DisplayArea displayArea = DisplayArea.GetFromWindowId(_appWindow.Id, DisplayAreaFallback.Nearest);
        RectInt32 workArea = displayArea.WorkArea;
        _appWindow.Resize(new SizeInt32(PanelWidth, PanelHeight));
        _appWindow.Move(new PointInt32(
            workArea.X + workArea.Width - PanelWidth - 14,
            workArea.Y + workArea.Height - PanelHeight - 14));
    }

    private void SyncViewFromState()
    {
        ClockText.Text = ViewModel.CurrentTime;
        StatusTitleText.Text = ViewModel.StatusTitle;
        StatusDetailText.Text = ViewModel.StatusDetail;
        StayActiveToggle.IsOn = ViewModel.IsEnabled;
        LaunchAtLoginToggle.IsOn = ViewModel.LaunchAtLogin;
        IntervalComboBox.SelectedItem = ViewModel.Interval;

        if (ViewModel.IsEnabled)
        {
            StatusIconBackground.Background = _accentBrush;
            StatusIcon.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 22, 25, 16));
            StatusIcon.Glyph = "\uE73E";
            StatusDot.Fill = _accentBrush;
            HeaderBolt.Foreground = _accentBrush;
        }
        else
        {
            StatusIconBackground.Background = _inactiveBrush;
            StatusIcon.Foreground = _inactiveForegroundBrush;
            StatusIcon.Glyph = "\uE769";
            StatusDot.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(72, 255, 255, 255));
            HeaderBolt.Foreground = (Brush)Application.Current.Resources["AntiAwaySecondaryBrush"];
        }

        ErrorPanel.Visibility = ViewModel.HasError ? Visibility.Visible : Visibility.Collapsed;
        ErrorText.Text = ViewModel.ErrorMessage ?? string.Empty;

        try
        {
            _trayIconService?.UpdateState(ViewModel.IsEnabled);
        }
        catch (Win32Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
        }
    }

    private void SyncOnboarding()
    {
        OnboardingPanel.Visibility = ViewModel.IsOnboardingVisible ? Visibility.Visible : Visibility.Collapsed;
        MainPanel.Visibility = ViewModel.IsOnboardingVisible ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        SyncViewFromState();
        if (args.PropertyName == nameof(AppViewModel.IsOnboardingVisible))
        {
            SyncOnboarding();
        }
    }

    private void StayActiveToggle_Toggled(object sender, RoutedEventArgs args)
    {
        if (_controlsReady && StayActiveToggle.IsOn != ViewModel.IsEnabled)
        {
            ViewModel.SetEnabled(StayActiveToggle.IsOn);
        }
    }

    private void IntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (_controlsReady && IntervalComboBox.SelectedItem is ActivityInterval interval && interval != ViewModel.Interval)
        {
            ViewModel.SetInterval(interval);
        }
    }

    private void LaunchAtLoginToggle_Toggled(object sender, RoutedEventArgs args)
    {
        if (_controlsReady && LaunchAtLoginToggle.IsOn != ViewModel.LaunchAtLogin)
        {
            ViewModel.SetLaunchAtLogin(LaunchAtLoginToggle.IsOn);
        }
    }

    private void DismissErrorButton_Click(object sender, RoutedEventArgs args)
    {
        ViewModel.DismissError();
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs args)
    {
        ViewModel.CompleteOnboarding();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs args)
    {
        if (_settingsWindow is null)
        {
            _settingsWindow = new SettingsWindow(ViewModel);
            _settingsWindow.WindowClosed += SettingsWindow_WindowClosed;
        }

        _settingsWindow.Activate();
    }

    private void SettingsWindow_WindowClosed(object? sender, EventArgs args)
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.WindowClosed -= SettingsWindow_WindowClosed;
            _settingsWindow = null;
        }
    }

    private void QuitButton_Click(object sender, RoutedEventArgs args)
    {
        App.CurrentApp.Shutdown();
    }

    private void TrayIconService_Activated(object? sender, EventArgs args)
    {
        if (_appWindow?.IsVisible == true)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (!_isExiting)
        {
            args.Cancel = true;
            HidePanel();
        }
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(nint windowHandle, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr(nint windowHandle, int index, nint newValue);
}

