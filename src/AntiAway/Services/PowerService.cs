using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AntiAway.Services;

public sealed class PowerService : IDisposable
{
    private const uint EsContinuous = 0x80000000;
    private const uint EsSystemRequired = 0x00000001;
    private bool _isHoldingSystemAwake;

    public void SetKeepAwake(bool enabled)
    {
        uint flags = enabled ? EsContinuous | EsSystemRequired : EsContinuous;
        if (SetThreadExecutionState(flags) == 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(),
                "Windows could not update the sleep-prevention state.");
        }

        _isHoldingSystemAwake = enabled;
    }

    public void Dispose()
    {
        if (_isHoldingSystemAwake)
        {
            _ = SetThreadExecutionState(EsContinuous);
            _isHoldingSystemAwake = false;
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SetThreadExecutionState(uint executionState);
}

