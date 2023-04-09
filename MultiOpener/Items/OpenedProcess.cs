using MultiOpener.Commands.OpenedCommands;
using MultiOpener.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MultiOpener.Items
{
    public class OpenedProcess : INotifyPropertyChanged
    {
        public IntPtr Hwnd { get; private set; }
        public IntPtr Handle { get; private set; }

        public ProcessStartInfo? ProcessStartInfo { get; private set; }

        public bool isMCInstance = false;

        private string? _windowTitle;
        public string? WindowTitle
        {
            get { return _windowTitle; }
            private set
            {
                _windowTitle = value;
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        public string? _status;
        public string? Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand ViewInformationsCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand CloseOpenCommand { get; private set; }


        public OpenedProcess()
        {
            ViewInformationsCommand = new OpenedViewInformationsCommand(this);
            ResetCommand = new OpenedResetCommand(this);
            CloseOpenCommand = new OpenedCloseOpenCommand(this);
        }

        public void SetHwnd(IntPtr hwnd)
        {
            Hwnd = hwnd;
            UpdateTitle();
        }
        public bool SetHwnd()
        {
            if (Hwnd != IntPtr.Zero) return true;

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

            UpdateStatus();
        }

        public void UpdateTitle()
        {
            if (!isMCInstance)
                WindowTitle = Path.GetFileNameWithoutExtension(ProcessStartInfo?.FileName);
            else
            {
                if (Hwnd == IntPtr.Zero) return;

                string title = Win32.GetWindowTitle(Hwnd);

                if (!string.IsNullOrEmpty(title))
                    WindowTitle = title;
            }

            if (string.IsNullOrEmpty(WindowTitle))
                WindowTitle = "Unknown";
        }
        public void UpdateStatus(string status = "")
        {
            if (string.IsNullOrEmpty(status))
            {
                if (ProcessStartInfo != null && Handle != IntPtr.Zero)
                    status = "OPENED";
                else
                    status = "CLOSED";
            }

            Status = "STATUS: " + status;
        }

        public bool HasWindow() => Handle != IntPtr.Zero;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task QuickOpen()
        {
            if (ProcessStartInfo == null) return;

            Process? process = Process.Start(ProcessStartInfo);

            if (process != null)
            {
                SetHandle(process.Handle);

                if (isMCInstance)
                {
                    //TODO: 1 ustawic hwnd dla odpalanej instancji na nowo
                    //nie jestem pewien tego, poniewaz ciezko bedzie znalesc ta konkretna teraz odpalana instancje, nawet jezeli porownam wszystkie odpalone co sie nie sprwadzi napewno
                    /*List<IntPtr> instances;
                    do
                    {
                        instances = Win32.GetWindowsByTitlePattern("Minecraft");
                        await Task.Delay(750);
                    }
                    while (instances.Count != 1);*/

                    //ze znalezionego ustawic tu hwnd
                    //current.SetHwnd(instances[i]);
                }
                else
                {
                    int errors = 0;
                    while (!SetHwnd() && errors < 10)
                    {
                        await Task.Delay(250);
                        errors++;
                    }
                }
            }
        }
    }
