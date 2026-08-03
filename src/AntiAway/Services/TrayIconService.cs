using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AntiAway.Services;

public sealed class TrayIconService : IDisposable
{
    private const uint NifMessage = 0x00000001;
    private const uint NifIcon = 0x00000002;
    private const uint NifTip = 0x00000004;
    private const uint NimAdd = 0x00000000;
    private const uint NimModify = 0x00000001;
    private const uint NimDelete = 0x00000002;
    private const uint ImageIcon = 1;
    private const uint LrLoadFromFile = 0x00000010;
    private const uint LrDefaultSize = 0x00000040;
    private const uint WmLButtonUp = 0x0202;
    private const uint WmRButtonUp = 0x0205;
    private const uint TrayMessage = 0x8000 + 41;
    private const nuint SubclassId = 0xA17A;

    private readonly nint _windowHandle;
    private readonly nint _iconHandle;
    private readonly SubclassProcedure _subclassProcedure;
    private readonly uint _taskbarCreatedMessage;
    private bool _isAdded;

    public TrayIconService(nint windowHandle, string iconPath)
    {
        _windowHandle = windowHandle;
        _subclassProcedure = WindowSubclassProcedure;
        _taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");

        _iconHandle = LoadImage(nint.Zero, iconPath, ImageIcon, 0, 0, LrLoadFromFile | LrDefaultSize);
        if (_iconHandle == nint.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "The AntiAway tray icon could not be loaded.");
        }

        if (!SetWindowSubclass(_windowHandle, _subclassProcedure, SubclassId, 0))
        {
            int error = Marshal.GetLastWin32Error();
            DestroyIcon(_iconHandle);
            throw new Win32Exception(error, "The AntiAway tray message handler could not be installed.");
        }

        AddIcon("AntiAway is off");
    }

    public event EventHandler? Activated;

    public void UpdateState(bool isEnabled)
    {
        NotifyIconData data = CreateData(isEnabled ? "AntiAway is active" : "AntiAway is off");
        if (!ShellNotifyIcon(NimModify, ref data))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "The AntiAway tray icon could not be updated.");
        }
    }

    public void Dispose()
    {
        if (_isAdded)
        {
            NotifyIconData data = CreateData(string.Empty);
            _ = ShellNotifyIcon(NimDelete, ref data);
            _isAdded = false;
        }

        _ = RemoveWindowSubclass(_windowHandle, _subclassProcedure, SubclassId);
        _ = DestroyIcon(_iconHandle);
    }

    private void AddIcon(string tooltip)
    {
        NotifyIconData data = CreateData(tooltip);
        if (!ShellNotifyIcon(NimAdd, ref data))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "The AntiAway tray icon could not be created.");
        }

        _isAdded = true;
    }

    private NotifyIconData CreateData(string tooltip) => new()
    {
        Size = (uint)Marshal.SizeOf<NotifyIconData>(),
        WindowHandle = _windowHandle,
        Id = 1,
        Flags = NifMessage | NifIcon | NifTip,
        CallbackMessage = TrayMessage,
        IconHandle = _iconHandle,
        Tip = tooltip,
        Info = string.Empty,
        InfoTitle = string.Empty
    };

    private nint WindowSubclassProcedure(
        nint windowHandle,
        uint message,
        nuint wordParameter,
        nint longParameter,
        nuint subclassId,
        nuint referenceData)
    {
        if (message == TrayMessage &&
            ((uint)longParameter == WmLButtonUp || (uint)longParameter == WmRButtonUp))
        {
            Activated?.Invoke(this, EventArgs.Empty);
            return nint.Zero;
        }

        if (message == _taskbarCreatedMessage)
        {
            try
            {
                _isAdded = false;
                AddIcon("AntiAway");
            }
            catch (Win32Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }

        return DefSubclassProc(windowHandle, message, wordParameter, longParameter);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NotifyIconData
    {
        public uint Size;
        public nint WindowHandle;
        public uint Id;
        public uint Flags;
        public uint CallbackMessage;
        public nint IconHandle;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Tip;

        public uint State;
        public uint StateMask;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Info;

        public uint TimeoutOrVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string InfoTitle;

        public uint InfoFlags;
        public Guid ItemGuid;
        public nint BalloonIconHandle;
    }

    private delegate nint SubclassProcedure(
        nint windowHandle,
        uint message,
        nuint wordParameter,
        nint longParameter,
        nuint subclassId,
        nuint referenceData);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "Shell_NotifyIconW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShellNotifyIcon(uint message, ref NotifyIconData data);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "LoadImageW")]
    private static extern nint LoadImage(
        nint instance,
        string name,
        uint type,
        int desiredWidth,
        int desiredHeight,
        uint loadFlags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(nint iconHandle);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint RegisterWindowMessage(string message);

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowSubclass(
        nint windowHandle,
        SubclassProcedure subclassProcedure,
        nuint subclassId,
        nuint referenceData);

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveWindowSubclass(
        nint windowHandle,
        SubclassProcedure subclassProcedure,
        nuint subclassId);

    [DllImport("comctl32.dll")]
    private static extern nint DefSubclassProc(
        nint windowHandle,
        uint message,
        nuint wordParameter,
        nint longParameter);
}
