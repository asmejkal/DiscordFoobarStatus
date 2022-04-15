using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordFoobarStatus.Core.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CopyData : IDisposable
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, ref CopyData target,
            User32.SendMessageTimeoutFlags fuFlags, uint uTimeout, out UIntPtr lpdwResult);

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

        public static CopyData CreateFromString(int dwData, string value, bool Unicode = false)
        {
            var result = new CopyData();
            result.dwData = (IntPtr)dwData;
            result.lpData = Unicode ? Marshal.StringToCoTaskMemUni(value) : Marshal.StringToCoTaskMemAnsi(value);
            result.cbData = (value.Length + 1) * (Unicode ? 2 : 1);
            return result;
        }

        public static CopyData CreateFromPtr(IntPtr lParam) => Marshal.PtrToStructure<CopyData>(lParam);
    }
}
