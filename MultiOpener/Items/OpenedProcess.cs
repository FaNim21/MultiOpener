using MultiOpener.Utils;
using System;
using System.Diagnostics;
using System.IO;

namespace MultiOpener.Items
{
    public class OpenedProcess
    {
        public IntPtr Hwnd { get; private set; }
        public IntPtr Handle { get; private set; }
        public string? WindowTitle { get; private set; }

        public ProcessStartInfo? ProcessStartInfo { get; private set; }

        public bool isMCInstance = false;


        public void SetHwnd(IntPtr hwnd)
        {
            Hwnd = hwnd;
            UpdateTitle();
        }
        public bool SetHwnd()
        {
            IntPtr output = Win32.GetHwndFromHandle(Handle);

            if (output != IntPtr.Zero)
            {
                Hwnd = output;
                UpdateTitle();
                return true;
            }
            return false;
        }

        public void SetHandle(IntPtr handle)
        {
            Handle = handle;
        }

        public void SetStartInfo(ProcessStartInfo startInfo)
        {
            ProcessStartInfo = startInfo;
        }

        public void UpdateTitle()
        {
            if (!isMCInstance)
            {
                WindowTitle = Path.GetFileNameWithoutExtension(ProcessStartInfo?.FileName);
                return;
            }

            if (Hwnd == IntPtr.Zero) return;

            string title = Win32.GetWindowTitle(Hwnd);

            if (!string.IsNullOrEmpty(title))
                WindowTitle = title;
        }

        public bool HasWindow() => Handle != IntPtr.Zero;
    }
}
