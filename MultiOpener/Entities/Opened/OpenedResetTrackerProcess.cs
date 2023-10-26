using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace MultiOpener.Entities.Opened;

/// <summary>
/// TODO: 0 Problemy z dodawaniem nowych typow procesu:
/// - Robienie oddzielnie: open, opened, modelView, opeView, openedView
/// - Dodawanie kazdego view oddzielnie za kazdym razem w App.xaml na zasadzie podlaczania viewModelu do view
/// - uzupelnianie dwoch switch casow w SettingsViewModel
/// - 
/// </summary>
public class OpenedResetTrackerProcess : OpenedProcess
{
    public override void Update(bool lookForWindow = false)
    {
        base.Update(lookForWindow);
    }
    public override void FindProcess()
    {

    }

    public override void UpdateTitle()
    {
        if (Hwnd == nint.Zero)
        {
            WindowTitle = System.IO.Path.GetFileName(Path);
            return;
        }

        string title = Win32.GetWindowTitle(Hwnd);
        if (!Win32.IsProcessResponding(Pid))
            title = "(Not Reponding)" + title;

        if (!string.IsNullOrEmpty(title))
            WindowTitle = title;

        if (string.IsNullOrEmpty(WindowTitle))
            WindowTitle = "Unknown";
    }

    public override async Task<bool> OpenProcess(CancellationToken token = default)
    {
        if (ProcessStartInfo == null) return false;

        await Task.Delay(100);

        Process? process;
        try
        {
            process = Process.Start(ProcessStartInfo);
        }
        catch (Exception)
        {
            StartViewModel.Log($"Cannot open MultiMC instance process '{Name}' from {ProcessStartInfo.WorkingDirectory}", ConsoleLineOption.Error);
            return false;
        }
        UpdateStatus();

        StartViewModel.Log($"Succesfully opened Reset Tracker '{Name}' at process ID: {Pid}");
        return true;
    }

    public override async Task<bool> Close()
    {
        //TODO: uwzglednic zatrzymywanie reset trackera jak jest uzywany wewnetrzny, a w przeciwnym przypadku zamyka proces klasycznie

        if (Pid == -1)
        {
            Clear();
            return true;
        }

        try
        {
            bool output = false;

            if (!Win32.IsProcessResponding(Pid))
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
            StartViewModel.Log($"Cannot close MC instance named {Name}(Title: {WindowTitle}) \n{e}", ConsoleLineOption.Error);
            return false;
        }
    }
}
