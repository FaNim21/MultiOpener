using MultiOpener.Commands.OpenedCommands;
using MultiOpener.Components.Controls;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiOpener.Entities.Opened;

public class OpenedProcess : INotifyPropertyChanged
{
    public string? Name { get; protected set; }
    public nint Hwnd { get; protected set; } = nint.Zero;
    public int Pid { get; protected set; } = -1;
    public string? Path { get; protected set; }
    public ProcessStartInfo? ProcessStartInfo { get; protected set; }

    private string? _windowTitle;
    public string? WindowTitle
    {
        get { return _windowTitle; }
        protected set
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
    public bool isMinimizeOnOpen = false;

    public ICommand ViewInformationsCommand { get; private set; }
    public ICommand ResetCommand { get; private set; }
    public ICommand CloseOpenCommand { get; private set; }
    public ICommand FocusCommand { get; private set; }


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
        FocusCommand = new OpenedFocusCommand(this, start);
    }

    public void Initialize(ProcessStartInfo? processStartInfo, string name, string path, bool isMinimizeOnOpen, int pid = -1)
    {
        this.isMinimizeOnOpen = isMinimizeOnOpen;
        Initialize(processStartInfo, name, path, pid);
    }
    public void Initialize(ProcessStartInfo? processStartInfo, string name, string path, int pid = -1)
    {
        ProcessStartInfo = processStartInfo;
        Name = name;
        Path = path;

        SetPid(pid);
    }

    public void SetPid(int pid)
    {
        Pid = pid;
        UpdateStatus();
    }
    public void SetPid()
    {
        int pid = -1;
        if (Hwnd != nint.Zero)
            pid = Win32.GetPidFromHwnd(Hwnd);

        if (pid <= 0)
        {
            if (!Win32.ProcessExist(Pid))
                Clear();
            return;
        }

        if (Pid <= 0 || Pid != pid)
            Pid = pid;
    }

    public void SetHwnd(nint hwnd)
    {
        Hwnd = hwnd;
        UpdateTitle();
    }
    public bool SetHwnd()
    {
        if (Pid == -1) return false;

        nint output = Win32.GetHwndFromPid(Pid);
        if (output == nint.Zero)
        {
            if (Hwnd != nint.Zero)
                SetHwnd(nint.Zero);
            return false;
        }

        SetHwnd(output);
        return true;
    }

    public virtual void FastUpdate()
    {
        if (!StillExist())
        {
            Clear();
            return;
        }

        UpdateTitle();
    }
    public virtual void Update()
    {
        bool exist = Win32.ProcessExist(Pid);
        bool windowExist = Win32.WindowExist(Hwnd);

        if (exist)
        {
            if (!windowExist)
                SetHwnd();
        }
        else
        {
            if (windowExist)
                SetPid();
            else
            {
                StartViewModel.Log($"{Name} has been closed or could not been found", ConsoleLineOption.Warning);
                Clear();
            }

            FindProcessByStartInfo();
            //TODO: 0 Tu zrobic szukanie procesu z podobnymi danymi ProcessStartInfo
        }

        UpdateTitle();
        UpdateStatus();
    }
    public virtual Task UpdateAsync(CancellationToken source = default) => Task.CompletedTask;

    public virtual void UpdateTitle()
    {
        if (!string.IsNullOrEmpty(WindowTitle)) return;

        string? title = System.IO.Path.GetFileNameWithoutExtension(ProcessStartInfo?.FileName);

        if (!Win32.IsProcessResponding(Pid))
            title = "(Not Responding) " + title;

        WindowTitle = title;
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
            StatusLabelColor = new BrushConverter().ConvertFromString("#7d2625") as SolidColorBrush;
        });
        Status = "CLOSED";
    }

    public bool IsOpenedFromStatus()
    {
        if (string.IsNullOrEmpty(Status))
            return false;

        return Status.Equals("OPENED");
    }
    public bool StillExist()
    {
        if (!IsOpenedFromStatus())
            return false;

        SetPid();

        if (Pid == -1)
            return false;

        if (!Win32.ProcessExist(Pid))
            return false;

        return true;
    }

    public void FindProcessByStartInfo()
    {
        //TODO: Zrobic to kiedy indziej bo jest tu duzo zaleznosci i problemow z javaw czy wieloma procesami chrome itp itd :d
        return;
        Process[] processes = Process.GetProcessesByName(ProcessStartInfo?.FileName);
        foreach (Process process in processes)
        {
            try
            {
                if (process.MainModule != null && process.StartInfo.FileName == ProcessStartInfo?.FileName && process.StartInfo.Arguments == ProcessStartInfo?.Arguments)
                {
                    SetPid(process.Id);
                    SetHwnd(process.MainWindowHandle);
                }
            }
            catch (Exception)
            {
                StartViewModel.Log($"Error occurred  looking for opened {Name}", ConsoleLineOption.Error);
            }
        }
    }

    public virtual async Task OpenProcess(CancellationToken token = default)
    {
        if (ProcessStartInfo == null) return;

        Process? process = null;
        try
        {
            process = Process.Start(ProcessStartInfo);
        }
        catch (Exception)
        {
            StartViewModel.Log($"Cannot open process {Name} from {ProcessStartInfo.WorkingDirectory}", ConsoleLineOption.Error);
        }

        if (process != null)
        {
            SetPid(process.Id);

            int errors = 0;
            int time = App.Config.TimeoutOpen / 15;
            bool isHwndFound = false;

            while (!isHwndFound && errors < 15)
            {
                if (token.IsCancellationRequested)
                    break;

                isHwndFound = SetHwnd();
                if (isHwndFound) break;

                await Task.Delay(time, CancellationToken.None);
                errors++;
            }
        }

        if (Hwnd != nint.Zero && isMinimizeOnOpen)
            Win32.MinimizeWindowHwnd(Hwnd);

        UpdateStatus();
    }
    public async Task<bool> Close()
    {
        if (Pid == -1)
        {
            Clear();
            return true;
        }

        try
        {
            bool output = false;

            Process process = Process.GetProcessById(Pid);

            if (!process.Responding)
                output = await Win32.CloseProcessByPid(Pid);

            if (!output)
            {
                output = await Win32.CloseProcessByHwnd(Hwnd);
                if (!output)
                    output = await Win32.CloseProcessByPid(Pid);
            }

            Clear();
            return output;
        }
        catch (Exception e)
        {
            DialogBox.Show($"Cannot close {WindowTitle ?? ""} \n{e}", "", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public void Clear()
    {
        SetPid(-1);
        SetHwnd(nint.Zero);
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