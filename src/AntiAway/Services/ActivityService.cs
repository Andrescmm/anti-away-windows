using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AntiAway.Services;

/// <summary>
/// Posts the narrowest useful Windows activity signal: a zero-distance mouse
/// move. It emits no clicks or keyboard input and does not change the pointer's
/// coordinates.
/// </summary>
public sealed class ActivityService
{
    private const uint InputMouse = 0;
    private const uint MouseEventMove = 0x0001;

    public void PostActivity()
    {
        Input[] inputs =
        [
            new Input
            {
                Type = InputMouse,
                Data = new InputUnion
                {
                    Mouse = new MouseInput
                    {
                        DeltaX = 0,
                        DeltaY = 0,
                        Flags = MouseEventMove
                    }
                }
            }
        ];

        uint inserted = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        if (inserted != inputs.Length)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(),
                "Windows could not send the local activity signal.");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int DeltaX;
        public int DeltaY;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int inputSize);
}

