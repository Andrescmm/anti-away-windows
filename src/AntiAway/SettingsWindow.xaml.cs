using System.ComponentModel;
using AntiAway.Models;
using AntiAway.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

namespace AntiAway;

public sealed partial class SettingsWindow : Window
{
    private readonly AppViewModel _viewModel;
    private AppWindow? _appWindow;
    private bool _controlsReady;
    private bool _isAppExiting;

    public SettingsWindow(AppViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        IntervalComboBox.ItemsSource = _viewModel.Intervals;
        SyncControls();
        _controlsReady = true;

        Activated += SettingsWindow_Activated;
        SystemBackdrop = new MicaBackdrop();
    }

    public event EventHandler? WindowClosed;

    public void CloseForAppExit()
    {
        _isAppExiting = true;
        Close();
    }

    private void SettingsWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (_appWindow is not null)
        {
            return;
        }

        nint windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        _appWindow.Resize(new SizeInt32(560, 650));
        _appWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "AntiAway.ico"));
        _appWindow.Closing += AppWindow_Closing;

        if (_appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = true;
            presenter.IsResizable = false;
        }
    }

    private void SyncControls()
    {
        StayActiveToggle.IsOn = _viewModel.IsEnabled;
        IntervalComboBox.SelectedItem = _viewModel.Interval;
        KeepAwakeToggle.IsOn = _viewModel.KeepComputerAwake;
        LaunchAtLoginToggle.IsOn = _viewModel.LaunchAtLogin;

        ErrorInfoBar.IsOpen = _viewModel.HasError;
        ErrorInfoBar.Message = _viewModel.ErrorMessage ?? string.Empty;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        SyncControls();
    }

    private void StayActiveToggle_Toggled(object sender, RoutedEventArgs args)
    {
        if (_controlsReady && StayActiveToggle.IsOn != _viewModel.IsEnabled)
        {
            _viewModel.SetEnabled(StayActiveToggle.IsOn);
        }
    }

    private void IntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (_controlsReady && IntervalComboBox.SelectedItem is ActivityInterval interval && interval != _viewModel.Interval)
        {
            _viewModel.SetInterval(interval);
        }
    }

    private void KeepAwakeToggle_Toggled(object sender, RoutedEventArgs args)
    {
        if (_controlsReady && KeepAwakeToggle.IsOn != _viewModel.KeepComputerAwake)
        {
            _viewModel.SetKeepComputerAwake(KeepAwakeToggle.IsOn);
        }
    }

    private void LaunchAtLoginToggle_Toggled(object sender, RoutedEventArgs args)
    {
        if (_controlsReady && LaunchAtLoginToggle.IsOn != _viewModel.LaunchAtLogin)
        {
            _viewModel.SetLaunchAtLogin(LaunchAtLoginToggle.IsOn);
        }
    }

    private void ShowWelcomeButton_Click(object sender, RoutedEventArgs args)
    {
        _viewModel.ShowOnboarding();
        Close();
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        if (!_isAppExiting)
        {
            WindowClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}

