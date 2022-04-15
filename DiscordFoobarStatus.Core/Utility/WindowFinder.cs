using System;
using System.Text;
using DiscordFoobarStatus.Core.Interop;

namespace DiscordFoobarStatus.Core.Utility
{
    public static class WindowFinder
    {
        public static bool TryFindProcessWindow(int targetProcessId, string title, out IntPtr handle)
        {
            IntPtr? result = null;
            User32.EnumWindows((hwnd, _) =>
            {
                User32.GetWindowThreadProcessId(hwnd, out var processId);
                if (processId == targetProcessId)
                {
                    var lpString = new StringBuilder(512);
                    if (User32.GetWindowText(hwnd, lpString, lpString.Capacity) != 0 && lpString.ToString() == title)
                        result = hwnd;
                }

                return result == null;
            }, IntPtr.Zero);

            handle = result ?? default;
            return result.HasValue;
        }
    }
}
