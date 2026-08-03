using AntiAway.Services;
using Microsoft.UI.Xaml;

namespace AntiAway;

public partial class App : Application
{
    private readonly SingleInstanceService _singleInstance = new("AntiAway.Desktop.SingleInstance");
    private MainWindow? _mainWindow;

    public static App CurrentApp => (App)Current;

    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (!_singleInstance.TryAcquire())
        {
            Exit();
            return;
        }

        _mainWindow = new MainWindow();
        _mainWindow.Activate();
        _mainWindow.InitializeAfterActivation();

        bool launchedAtStartup = args.Arguments
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Any(argument => argument.Equals("--startup", StringComparison.OrdinalIgnoreCase));

        if (!launchedAtStartup || _mainWindow.ViewModel.IsOnboardingVisible)
        {
            _mainWindow.ShowPanel();
        }
        else
        {
            _mainWindow.HidePanel();
        }
    }

    public void Shutdown()
    {
        _mainWindow?.Dispose();
        _mainWindow = null;
        _singleInstance.Dispose();
        Exit();
    }

    private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
    {
        System.Diagnostics.Debug.WriteLine(args.Exception);
    }
}

