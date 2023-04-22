using MultiOpener.Commands.OpenedCommands;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiOpener.Items
{
    public class OpenedProcess : INotifyPropertyChanged
    {
        public const string MCPattern = @"Minecraft\*\s+(\s+-\s+instance)?\s*(?:\d+(\.\d+)+|\d+)";

        public IntPtr Hwnd { get; private set; }
        public IntPtr Handle { get; private set; }
        public int Pid { get; private set; }

        public string? Path { get; private set; }

        public ProcessStartInfo? ProcessStartInfo { get; private set; }

        public bool isMCInstance = false;
        public int mcInstancesAmount = 0;

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

        private Brush _color;
        public Brush StatusLabelColor
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged(nameof(StatusLabelColor));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand ViewInformationsCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand CloseOpenCommand { get; private set; }

        /*
         IsEnabled="{Binding IsContextMenuEnabled}"
         Visibility="{Binding IsContextMenuEnabled, Converter={StaticResource BoolToVisibilityConverter}, Mode=OneWay}"
        */
        public bool IsContextMenuEnabled { get { return !Consts.IsStartPanelWorkingNow; } private set { } }


        public OpenedProcess()
        {
            StartViewModel start = null;

            Application.Current.Dispatcher.Invoke(delegate
            {
                start = ((MainWindow)Application.Current.MainWindow).MainViewModel.start;
            });

            ViewInformationsCommand = new OpenedViewInformationsCommand(this);
            ResetCommand = new OpenedResetCommand(this, start);
            CloseOpenCommand = new OpenedCloseOpenCommand(this, start);
        }

        //TODO: 7 moze cos typu CanSetHwnd? zeby sprawdzic czy proces jest non gui
        public void SetHwnd(IntPtr hwnd)
        {
            Hwnd = hwnd;
            UpdateTitle();
            SetPid();
        }
        public bool SetHwnd()
        {
            if (Handle == IntPtr.Zero || Pid == -1) return false;

            IntPtr output = Win32.GetHwndFromHandle(Handle);
            if (output == IntPtr.Zero)
                output = Win32.GetHwndFromPid(Pid);

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

        public void SetPath(string path = null)
        {
            if (isMCInstance)
                Path = Win32.GetJavaFilePath(Pid);

            if (!string.IsNullOrEmpty(path))
                Path = path;
        }

        public void SetStartInfo(ProcessStartInfo startInfo)
        {
            ProcessStartInfo = startInfo;

            UpdateStatus();
        }

        public void FastUpdate()
        {
            SetPid();
            UpdateStatus();
        }
        public void Update()
        {
            //might be expensive because of externs that is used here and stupid amount of things to check that needs to be changed etc
            //TODO: 9 OPTIMIZE IT

            if (StillExist())
            {
                UpdateTitle();
                return;
            }

            if (!StillExist() && Handle != IntPtr.Zero)
            {
                UpdateStatus("CLOSED");
                ClearAfterClose();
            }

            SetHwnd();
            SetPid();

            if (Handle == IntPtr.Zero && Pid != -1)
                SetHandle(Win32.GetHandleFromPid(Pid));

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
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        StatusLabelColor = new BrushConverter().ConvertFromString("#33cc33") as SolidColorBrush;
                    });
                    status = "OPENED";
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        StatusLabelColor = new BrushConverter().ConvertFromString("#cc3300") as SolidColorBrush;
                    });
                    status = "CLOSED";
                }
            }

            Status = status;
        }

        public bool IsOpened()
        {
            if (string.IsNullOrEmpty(Status))
                return false;

            return Status.Equals("OPENED");
        }
        public bool StillExist()
        {
            if (Status == "CLOSED")
                return false;

            SetPid();

            if (Pid == -1)
                return false;

            if (!Win32.ProcessExist(Pid))
                return false;

            return true;
        }
        public bool HasWindow() => Hwnd != IntPtr.Zero;

        public async Task QuickOpen()
        {
            if (ProcessStartInfo == null) return;

            Process? process = Process.Start(ProcessStartInfo);

            if (process != null)
            {
                if (isMCInstance)
                {
                    process.WaitForInputIdle();

                    bool isSuccessful = await SearchForMCInstance();
                    if (isSuccessful)
                    {
                        IntPtr handle = Win32.GetHandleFromPid(Pid);
                        if (handle != IntPtr.Zero)
                            SetHandle(handle);
                    }
                }
                else
                {
                    SetHandle(process.Handle);
                    SetPid();
                    int errors = 0;
                    while (!SetHwnd() && errors < 15)
                    {
                        await Task.Delay(250);
                        errors++;
                    }
                }
            }

            UpdateStatus();
        }
        public async Task<bool> SearchForMCInstance()
        {
            Regex mcPatternRegex = new(MCPattern);
            List<IntPtr> instances;
            int errorCount = -1;

            bool isHwndFound = false;
            do
            {
                errorCount++;
                await Task.Delay(1000);

                instances = Win32.GetWindowsByTitlePattern(mcPatternRegex);
                for (int i = 0; i < instances.Count; i++)
                {
                    int currentPid = (int)Win32.GetPidFromHwnd(instances[i]);
                    string currentPath = Win32.GetJavaFilePath(currentPid);

                    if (currentPath.Equals(Path))
                    {
                        isHwndFound = true;
                        SetHwnd(instances[i]);
                        Pid = currentPid;
                        break;
                    }
                }
            } while (!isHwndFound && errorCount < 15);

            return errorCount < 15;
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

        public void ClearAfterClose()
        {
            Handle = IntPtr.Zero;
            Hwnd = IntPtr.Zero;
            Pid = -1;
        }

        public override string ToString()
        {
            if (ProcessStartInfo == null)
                return "";

            return $"{WindowTitle}\n" +
                   $"PID: {Pid}\n" +
                   $"StartInfo: FileName: {ProcessStartInfo.FileName}\n" +
                   $"           WokrkingDirectory: {ProcessStartInfo.WorkingDirectory}\n" +
                   $"           Path: {ProcessStartInfo.Arguments}\n" +
                   $"Path: {Path}\n" +
                   $"Handle: {Handle}\n" +
                   $"Hwnd: {Hwnd}";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
