using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Utils;

public class InputController
{
    public bool IsShiftPressed
    {
        get
        {
            bool output = false;
            Application.Current?.Dispatcher.Invoke(delegate
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    output = true;
            });
            return output;
        }
        private set { }
    }

    public bool IsCtrlPressed
    {
        get
        {
            bool output = false;
            Application.Current?.Dispatcher.Invoke(delegate
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    output = true;
            });
            return output;
        }
        private set { }
    }


    //TODO 9 w przyszlosci przerobic to na forme jakiejs petli/aktualizacji przy rzeczywistej potrzebie sprawdzania?
    private void UpdateInputs() { }
}
