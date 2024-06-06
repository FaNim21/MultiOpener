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
    public ObservableCollection<ConsoleLine> ConsoleLines { get; init; }

    private readonly string _logPath;


    public ConsoleViewModel()
    {
        ConsoleLines = new ObservableCollection<ConsoleLine>();

        _logPath = Path.Combine(Consts.AppdataPath, "Logs");
        if (!Directory.Exists(_logPath)) Directory.CreateDirectory(_logPath);
    }

    public void ProcessCommandLine(string text, ConsoleLineOption option = ConsoleLineOption.Normal)
    {
        Application.Current?.Dispatcher.Invoke(delegate
        {
            SolidColorBrush brush;
            StringBuilder sb = new();

            var time = DateTime.Now.ToString("HH:mm:ss");
            sb.Append("[" + time);

            switch (option)
            {
                case ConsoleLineOption.Error:
                    sb.Append(" - ERROR]> ");
                    brush = Brushes.LightCoral;
                    break;
                case ConsoleLineOption.Warning:
                    sb.Append(" - WARNING]> ");
                    brush = Brushes.Yellow;
                    break;
                case ConsoleLineOption.Normal:
                default:
                    sb.Append("]> ");
                    brush = Brushes.White;
                    break;
            }
            sb.Append(text);

            var consoleLine = new ConsoleLine() { Text = sb.ToString(), Color = brush };
            ConsoleLines.Add(consoleLine);
        });

        OnPropertyChanged(nameof(ConsoleLines));
    }

    public void Save(string logName = "log")
    {
        string date = DateTime.Now.ToString("yyyy-MM-dd_HH.mm");
        string fileName = $"{logName} {date}.txt";

        int count = 1;
        while (File.Exists(_logPath + "\\" + fileName))
        {
            fileName = $"{logName} {date} [{count}].txt";
            count++;
        }

        List<string> lines = new();
        foreach (var line in ConsoleLines)
        {
            if (string.IsNullOrEmpty(line.Text)) continue;
            lines.Add(line.Text);
        }

        File.WriteAllLines(_logPath + "\\" + fileName, lines);
    }

    public void Clear()
    {
        ConsoleLines.Clear();
    }
}
