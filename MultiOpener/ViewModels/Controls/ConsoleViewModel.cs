using MultiOpener.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MultiOpener.ViewModels.Controls;


public class ConsoleViewModel : BaseViewModel
{
    public ObservableCollection<ConsoleLine> ConsoleLines { get; set; }

    private readonly string logPath;

    public ConsoleViewModel()
    {
        ConsoleLines = new ObservableCollection<ConsoleLine>();

        logPath = Path.Combine(Consts.AppdataPath, "Logs");

        if (!Directory.Exists(logPath))
            Directory.CreateDirectory(logPath);
    }

    public void ProcessCommandLine(string text, ConsoleLineOption option = ConsoleLineOption.Normal)
    {
        Application.Current?.Dispatcher.Invoke(delegate
        {
            var consoleLine = new ConsoleLine();

            StringBuilder sb = new();
            var time = DateTime.Now.ToString("HH:mm:ss");
            sb.Append("[" + time);

            if (option == ConsoleLineOption.Error)
            {
                sb.Append(" - ERROR]> ");
                consoleLine.Color = Brushes.Red;
            }
            else if (option == ConsoleLineOption.Warning)
            {
                sb.Append(" - WARNING]> ");
                consoleLine.Color = Brushes.Yellow;
            }
            else
            {
                sb.Append("]> ");
                consoleLine.Color = Brushes.White;
            }

            sb.Append(text);
            consoleLine.Text = sb.ToString();

            ConsoleLines?.Add(consoleLine);
            OnPropertyChanged(nameof(ConsoleLines));
        });
    }

    public void Save()
    {
        string date = DateTime.Now.ToString("yyyy-MM-dd_HH.mm");
        string fileName = $"{date}.txt";

        int count = 1;
        while (File.Exists(logPath + "\\" + fileName))
        {
            fileName = $"{date} [{count}].txt";
            count++;
        }

        List<string> lines = new();
        foreach (var line in ConsoleLines)
        {
            if (string.IsNullOrEmpty(line.Text)) continue;
            lines.Add(line.Text);
        }

        File.WriteAllLines(logPath + "\\" + fileName, lines);
    }

    public void Clear()
    {
        ConsoleLines?.Clear();
    }
}
