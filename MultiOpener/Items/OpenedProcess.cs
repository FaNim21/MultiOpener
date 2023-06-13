using MultiOpener.Commands.OpenedCommands;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiOpener.Items;

public class OpenedProcess : INotifyPropertyChanged
{
    public const string MCPattern = @"Minecraft\*\s+(\s+-\s+instance)?\s*(?:\d+(\.\d+)+|\d+)";

    public string? Name { get; private set; }

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

    private Brush? _color;
    public Brush? StatusLabelColor
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


    public OpenedProcess(StartViewModel? start = null)
    {
        if (start == null)
        {
            Application.Current?.Dispatcher.Invoke(delegate
            {
                start = ((MainWindow)Application.Current.MainWindow).MainViewModel.start;
            });
        }

        ViewInformationsCommand = new OpenedViewInformationsCommand(this);
        ResetCommand = new OpenedResetCommand(this, start);
        CloseOpenCommand = new OpenedCloseOpenCommand(this, start);
    }
    public void Initialize(ProcessStartInfo? processStartInfo, string name, IntPtr handle, string path)
    {
        ProcessStartInfo = processStartInfo;
        Name = name;
        Handle = handle;
        Path = path;

        UpdateStatus();
    }

    public void SetHandle(IntPtr handle)
    {
        Handle = handle;
    }

    public void SetHwnd(IntPtr hwnd)
    {
        Hwnd = hwnd;
        UpdateTitle();
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

        Hwnd = output;
        UpdateTitle();
        SetPid();
        return true;
    }

    public void SetPid()
    {
        int pid;
        if (Hwnd != IntPtr.Zero)
            pid = (int)Win32.GetPidFromHwnd(Hwnd);
        else if (Handle != IntPtr.Zero)
            pid = (int)Win32.GetPidFromHandle(Handle);
        else
            pid = -1;

        if (pid <= 0) return;

        if (pid == -1)
        {
            Pid = pid;
            return;
        }

        if (Pid == 0 || isMCInstance || Pid != pid)
            Pid = pid;
    }

    public void FastUpdate()
    {
        SetPid();
        UpdateStatus();
    }
    public void Update()
    {
        if (StillExist())
        {
            UpdateTitle();
            return;
        }

        if (!StillExist() && Handle != IntPtr.Zero)
        {
            Clear();
            UpdateStatus();
            return;
        }

        SetHwnd();
        SetPid();

        if (Handle == IntPtr.Zero && Pid != -1)
            SetHandle(Win32.GetHandleFromPid(Pid));

        UpdateTitle();
        UpdateStatus();
    }

    public void UpdateTitle()
    {
        if (!isMCInstance && string.IsNullOrEmpty(WindowTitle))
            WindowTitle = System.IO.Path.GetFileNameWithoutExtension(ProcessStartInfo?.FileName);
        else if (isMCInstance)
        {
            if (Hwnd == IntPtr.Zero) return;

            string title = Win32.GetWindowTitle(Hwnd);

            if (!string.IsNullOrEmpty(title))
                WindowTitle = title;
        }

        if (string.IsNullOrEmpty(WindowTitle))
            WindowTitle = "Unknown";
    }
    public void UpdateStatus()
    {
        if (Pid != -1)
        {
            Application.Current?.Dispatcher.Invoke(delegate
            {
                StatusLabelColor = new BrushConverter().ConvertFromString("#33cc33") as SolidColorBrush;
            });
            Status = "OPENED";
            return;
        }

        Application.Current?.Dispatcher.Invoke(delegate
        {
            StatusLabelColor = new BrushConverter().ConvertFromString("#cc3300") as SolidColorBrush;
        });
        Status = "CLOSED";
    }

    public bool IsOpened()
    {
        if (string.IsNullOrEmpty(Status))
            return false;

        return Status.Equals("OPENED");
    }
    public bool StillExist()
    {
        if (!IsOpened())
            return false;

        SetPid();

        if (Pid == -1)
            return false;

        if (!Win32.ProcessExist(Pid))
            return false;

        return true;
    }

    public async Task QuickOpen()
    {
        if (ProcessStartInfo == null) return;

        Process? process = null;
        try
        {
            process = Process.Start(ProcessStartInfo);
        }
        catch { }

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
                //TODO: 2 customize this timeout in options (waiting for hwnd in quick open)
            }
        }

        UpdateStatus();
    }
    public async Task<bool> Close()
    {
        if (Pid == -1) return true;

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

    public async Task<bool> SearchForMCInstance()
    {
        Regex mcPatternRegex = new(MCPattern);
        List<IntPtr> instances;
        int errorCount = -1;
        //var config = new TimeoutConfigurator(App.config.TimeLookingForInstancesData, 50);

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
        //TODO: 2 customize this timeout in options (looking for single instance in start panel)

        return errorCount < 15;
    }

    public void Clear()
    {
        Handle = IntPtr.Zero;
        Hwnd = IntPtr.Zero;
        Pid = -1;
    }

    public override string ToString()
    {
        if (ProcessStartInfo == null)
            return "";

        StringBuilder sb = new();

        sb.AppendLine($"Name: {Name}");
        if (string.IsNullOrEmpty(WindowTitle))
            sb.AppendLine($"Title: 'Not loaded yet'");
        else if (!WindowTitle!.ToLower().Equals(Name!.ToLower()))
            sb.AppendLine($"Title: {WindowTitle}");
        sb.AppendLine($"Path: {Path}");
        sb.AppendLine($"ID: {Pid}");
        sb.AppendLine($"Handle: {Handle.ToString()}");
        sb.AppendLine($"Hwnd: {Hwnd.ToString()}");
        sb.AppendLine("");
        sb.AppendLine(" -- StartInfo -- ");
        if (!string.IsNullOrEmpty(ProcessStartInfo.FileName))
            sb.AppendLine($"FileName: {ProcessStartInfo.FileName}");
        if (!string.IsNullOrEmpty(ProcessStartInfo.WorkingDirectory))
            sb.AppendLine($"WokrkingDirectory: {ProcessStartInfo.WorkingDirectory}");
        if (!string.IsNullOrEmpty(ProcessStartInfo.Arguments))
            sb.AppendLine($"Arguments: {ProcessStartInfo.Arguments}");

        return sb.ToString();
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}