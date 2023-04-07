using MultiOpener.Utils;
using System;

namespace MultiOpener.Items
{
    public class OpenedProcess
    {
        public IntPtr Hwnd { get; private set; }
        public IntPtr Handle { get; private set; }
        public string? WindowTitle { get; private set; }


        public bool IsMinecraftInstance = false;

        //TODO: dodac informacje typu sciezka do odpalenia pliku jeszcze raz, i wiecej


        public void SetHwnd(IntPtr hwnd)
        {
            Hwnd = hwnd;
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

        public void UpdateTitle()
        {
            if (Hwnd == IntPtr.Zero) return;

            string title = Win32.GetWindowTitle(Hwnd);

            if (!string.IsNullOrEmpty(title))
            {
                WindowTitle = title;
            }
        }

        public bool HasWindow() => Handle != IntPtr.Zero;
    }
}
