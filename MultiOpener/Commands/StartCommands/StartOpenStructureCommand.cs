using MultiOpener.ViewModels;
using System.Diagnostics;

namespace MultiOpener.Commands.StartCommands
{
    public class StartOpenStructureCommand : StartCommandBase
    {
        public StartOpenStructureCommand(StartViewModel? startViewModel) : base(startViewModel)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Start == null) return;

            Trace.WriteLine("Otwiera szkielet presetu");
        }
    }
}
