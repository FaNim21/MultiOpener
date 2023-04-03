﻿using MultiOpener.Commands.StartCommands;
using System;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    public class StartViewModel : BaseViewModel
    {
        public ICommand OpenCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        private bool _openButtonEnabled = true;
        public bool OpenButtonEnabled
        {
            get { return _openButtonEnabled; }
            set
            {
                _openButtonEnabled = value;
                OnPropertyChanged(nameof(OpenButtonEnabled));
            }
        }

        private string? _presetNameLabel;
        public string? PresetNameLabel
        {
            get { return _presetNameLabel; }
            set
            {
                _presetNameLabel = value;
                OnPropertyChanged(nameof(PresetNameLabel));
            }
        }

        public StartViewModel()
        {
            OpenCommand = new StartOpenCommand(this);
            CloseCommand = new StartCloseCommand(this);

            PresetNameLabel = "Empty preset";
        }
    }
}
