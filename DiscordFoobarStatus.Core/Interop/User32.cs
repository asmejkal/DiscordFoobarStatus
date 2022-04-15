using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscordFoobarStatus.Core.Interop
{
    public static class User32
    {
        public const uint WM_COPYDATA = 0x4A;

        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8
        }

        [DllImport("User32.dll")]
        public static extern int SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags fuFlags, uint timeout, out UIntPtr lpdwResult);

        public delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("User32.dll", EntryPoint = "RegisterWindowMessageW", SetLastError = true)]
        public static extern uint RegisterWindowMessage(string lpString);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder lptrString, int nMaxCount);

        [DllImport("User32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    }
}
