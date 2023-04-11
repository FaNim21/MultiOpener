using MultiOpener.Commands.OpenedCommands;
using MultiOpener.Utils;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Policy;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Items
{
    public class OpenedProcess : INotifyPropertyChanged
    {
        public IntPtr Hwnd { get; private set; }
        public IntPtr Handle { get; private set; }
        public int Pid { get; private set; }

        public string? Path { get; private set; }

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
            SetPid();
        }
        public bool SetHwnd()
        {
            if (Handle == IntPtr.Zero) return false;

            IntPtr output = Win32.GetHwndFromHandle(Handle);

            if (output == IntPtr.Zero)
            {
                Hwnd = IntPtr.Zero;
                return false;
            }
            else
            {
                Hwnd = output;
                UpdateTitle();
                SetPid();
                return true;
            }
        }

        public void SetHandle(IntPtr handle)
        {
            Handle = handle;
        }
        public void SetPid()
        {
            if (Hwnd != IntPtr.Zero)
            {
                int pid = (int)Win32.GetPidFromHwnd(Hwnd);
                if (pid == 0)
                    return;

                if (Pid == 0 || isMCInstance || Pid != pid)
                    Pid = pid;
            }
            else if (Handle != IntPtr.Zero)
            {
                int pid = (int)Win32.GetPidFromHandle(Handle);
                if (pid == 0)
                    return;

                if (Pid != pid)
                    Pid = pid;
            }
            else
                Pid = -1;
        }

        public void SetPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
                Path = path;
        }

        public void SetStartInfo(ProcessStartInfo startInfo)
        {
            ProcessStartInfo = startInfo;

            UpdateStatus();
        }

        public void Update()
        {
            if (!StillExist() && Handle != IntPtr.Zero)
            {
                UpdateStatus("CLOSED");
                ClearAfterClose();
            }

            SetHwnd();
            SetPid();
            UpdateStatus();
            UpdateTitle();
        }
        public void UpdateTitle()
        {
            if (!isMCInstance)
                WindowTitle = System.IO.Path.GetFileNameWithoutExtension(ProcessStartInfo?.FileName);
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
                if (Pid != -1)
                    status = "OPENED";
                else
                    status = "CLOSED";
            }

            Status = "STATUS: " + status;
        }

        public bool IsOpened()
        {
            return Status.Equals("STATUS: OPENED");
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
                    MessageBox.Show("NOT IMPLEMENTED YET");

                    //MessageBox.Show("Refresh after instance will be fully opened");
                    //TODO: 1 ustawic hwnd dla odpalanej instancji na nowo
                    //uzyc do tego rozpoznawania procesow po reszcie hwnd albo po pid
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

            UpdateStatus();
        }

        public async Task<bool> Close()
        {
            if (Pid == -1)
                return true;

            try
            {
                bool output = await Win32.CloseProcessByHwnd(Hwnd);
                if (!output)
                {
                    output = await Win32.CloseProcessByPid(Pid);
                    if (!output)
                    {
                        output = await Win32.CloseProcessByPid((int)Win32.GetPidFromHandle(Handle));
                    }
                }
                return output;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Cannot close {WindowTitle ?? ""} \n{e}");
                return false;
            }
        }

        public bool StillExist()
        {
            if (Status == "STATUS: CLOSED")
                return false;

            SetPid();

            if (Pid == -1)
                return false;

            if (!Win32.ProcessExist(Pid))
                return false;

            return true;
        }

        public void ClearAfterClose()
        {
            Handle = IntPtr.Zero;
            Hwnd = IntPtr.Zero;
            Pid = -1;
        }
    }
}
