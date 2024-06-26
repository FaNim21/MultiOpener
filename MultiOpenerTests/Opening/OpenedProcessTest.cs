﻿using MultiOpener;
using MultiOpener.Commands.OpenedCommands;
using MultiOpener.Entities.Opened;
using System.Diagnostics;

namespace MultiOpenerTests.Opening;

[TestFixture]
internal class OpenedProcessTest
{
    public OpenedProcess? opened;

    private const string _pathExe = "C:\\Users\\Filip\\Desktop\\Julti.jar";

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        try
        {
            string executable = Path.GetFileName(_pathExe);
            string pathDir = Path.GetDirectoryName(_pathExe) ?? "";

            ProcessStartInfo startInfo = new() { WorkingDirectory = pathDir, FileName = executable, UseShellExecute = true };
            Process? process = Process.Start(startInfo);

            if (process != null)
            {
                OpenedProcess opened = new(null);
                string? name = Path.GetFileNameWithoutExtension(startInfo?.FileName);
                opened.Initialize(startInfo, name!, _pathExe, false, process.Id);

                int errors = 0;
                while (!opened.SetHwnd() && errors < 15)
                {
                    await Task.Delay(250);
                    errors++;
                }

                this.opened = opened;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (opened == null) return;

        if (!opened.IsOpenedFromStatus())
        {
            opened = null;
            return;
        }

        bool result = await opened.Close();
        if (result && !opened.IsOpenedFromStatus())
            opened = null;
    }


    [Test, Order(0)]
    public void IsOpened_ReturnsTrue()
    {
        bool result = opened!.IsOpenedFromStatus();
        Assert.That(result, Is.True);
    }

    [Test, Order(1)]
    public void UpdateOpened_WorkSuccesfully()
    {
        opened!.Update();
        bool result = opened!.IsOpenedFromStatus();
        Assert.That(result, Is.True);
    }

    [Test, Order(2)]
    public async Task RestartOpened_Succesfully()
    {
        opened!.Update();
        await ((OpenedResetCommand)opened!.ResetCommand).ResetOpened();
        bool result = !Consts.IsStartPanelWorkingNow && opened!.IsOpenedFromStatus();
        Assert.That(result, Is.True);
    }

    [Test, Order(3)]
    public async Task CloseOpened_Succesfully()
    {
        opened!.Update();
        await ((OpenedCloseOpenCommand)opened!.CloseOpenCommand).CloseOpenOpened();
        opened!.Update();
        bool result = !Consts.IsStartPanelWorkingNow && !opened!.IsOpenedFromStatus();
        Assert.That(result, Is.True);
    }

    [Test, Order(4)]
    public async Task OpenOpened_Succesfully()
    {
        opened!.Update();
        await ((OpenedCloseOpenCommand)opened!.CloseOpenCommand).CloseOpenOpened();
        bool result = !Consts.IsStartPanelWorkingNow && opened!.IsOpenedFromStatus();
        Assert.That(result, Is.True);
    }
}