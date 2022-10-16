using System;
using System.Runtime.InteropServices;

namespace DiscordFoobarStatus.Utility
{
    public static class User32
    {
        [DllImport("User32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    }
}
