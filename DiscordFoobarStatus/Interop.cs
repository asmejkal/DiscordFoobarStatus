using System;
using System.Runtime.InteropServices;

namespace DiscordFoobarStatus
{
    public static class Interop
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CopyData : IDisposable
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
            static extern int SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, ref CopyData target,
                SendMessageTimeoutFlags fuFlags, uint uTimeout, out UIntPtr lpdwResult);

            [Flags]
            enum SendMessageTimeoutFlags : uint
            {
                SMTO_NORMAL = 0x0,
                SMTO_BLOCK = 0x1,
                SMTO_ABORTIFHUNG = 0x2,
                SMTO_NOTIMEOUTIFNOTHUNG = 0x8
            }

            const uint WM_COPYDATA = 0x4A;

            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;

            public void Dispose()
            {
                if (lpData != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(lpData);
                    lpData = IntPtr.Zero;
                    cbData = 0;
                }
            }

            public string AsAnsiString => Marshal.PtrToStringAnsi(lpData, cbData);

            public string? AsUnicodeString => Marshal.PtrToStringUni(lpData);

            public static CopyData CreateForString(int dwData, string value, bool Unicode = false)
            {
                var result = new CopyData();
                result.dwData = (IntPtr)dwData;
                result.lpData = Unicode ? Marshal.StringToCoTaskMemUni(value) : Marshal.StringToCoTaskMemAnsi(value);
                result.cbData = (value.Length + 1) * (Unicode ? 2 : 1);
                return result;
            }

            public static int Send(IntPtr targetHandle, int dwData, string value, uint timeoutMs = 1000, bool Unicode = false)
            {
                var cds = CreateForString(dwData, value, Unicode);
                var status = SendMessageTimeout(targetHandle, WM_COPYDATA, IntPtr.Zero, ref cds, SendMessageTimeoutFlags.SMTO_NORMAL, timeoutMs, out _);
                var result = status != 0 ? Marshal.GetLastWin32Error() : 0;
                cds.Dispose();
                return result;
            }
        }

        [DllImport("User32.dll")]
        public static extern int SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint timeout, out UIntPtr lpdwResult);

        private delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr lParam);

        [DllImport("user32")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        
    }
}
