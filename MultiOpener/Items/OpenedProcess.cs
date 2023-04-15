﻿using MultiOpener.Commands.OpenedCommands;
using MultiOpener.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
            //might be expensive because of externs that is used here
            //TODO: 9 Optimize it

            if (StillExist())
                return;

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
        public bool HasWindow() => Handle != IntPtr.Zero;

        public async Task QuickOpen()
        {
            if (ProcessStartInfo == null) return;

            Process? process = Process.Start(ProcessStartInfo);

            if (process != null)
            {
                SetHandle(process.Handle);

                if (isMCInstance)
                {
                    process.WaitForInputIdle();

                    //TODO: 0 NAPRAWIC TO - TRACE PROCESY MC PLUS PRZY ODPALANIU PROCESU ZE STARMY PROCESSSTARTINFO ODPALA MI LOG KONSOLE Z MULTIMC PLUS COS TAM JESZCZE
                    bool isSuccessful = await SearchForMCInstance();
                    if (isSuccessful)
                        SetPid();
                }
                else
                {
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
            int errorCount = 0;

            bool isHwndFound = false;
            int checkedAmount = 0;
            do
            {
                instances = Win32.GetWindowsByTitlePattern(mcPatternRegex);
                for (int i = checkedAmount; i < instances.Count; i++, checkedAmount++)
                {
                    int currentPid = (int)Win32.GetPidFromHwnd(instances[i]);

                    if (Win32.GetJavaFilePath(currentPid).Equals(Path))
                    {
                        isHwndFound = true;
                        SetHwnd(instances[i]);
                        Pid = currentPid;
                    }
                }
                await Task.Delay(750);
                errorCount++;
            } while (!isHwndFound && errorCount < 20);

            return errorCount < 20;
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
            return $"{WindowTitle}\n" +
                   $"PID: {Pid}\n" +
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
