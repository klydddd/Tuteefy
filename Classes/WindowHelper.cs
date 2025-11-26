using System.Windows;
using System.Windows.Media;

namespace TuteefyWPF.Classes
{
    public static class WindowHelper
    {
        private static Window _dimmedWindow;
        private static Brush _originalBackground;

        // Overload 1: ShowDimmedDialog with parent and dialog windows (for existing code)
        public static void ShowDimmedDialog(Window parentWindow, Window dialogWindow)
        {
            if (parentWindow != null)
            {
                _dimmedWindow = parentWindow;
                _originalBackground = _dimmedWindow.Background;

                // Create semi-transparent dark overlay
                _dimmedWindow.Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
                _dimmedWindow.IsEnabled = false;
            }
        }

        // Overload 2: ShowDimmedDialog with just dialog window (auto-find parent)
        public static void ShowDimmedDialog(Window dialogWindow)
        {
            // Find the parent window automatically
            foreach (Window window in Application.Current.Windows)
            {
                if (window != dialogWindow && window.IsActive)
                {
                    ShowDimmedDialog(window, dialogWindow);
                    break;
                }
            }
        }

        // Overload 1: UndimDialog with window parameter (for existing code)
        public static void UndimDialog(Window window)
        {
            UndimDialog();
        }

        // Overload 2: UndimDialog with no parameters
        public static void UndimDialog()
        {
            if (_dimmedWindow != null)
            {
                _dimmedWindow.Background = _originalBackground;
                _dimmedWindow.IsEnabled = true;
                _dimmedWindow = null;
            }
        }

        // Legacy method names for compatibility
        public static void DimBackgroundWindow(Window activeWindow)
        {
            ShowDimmedDialog(activeWindow);
        }

        public static void RemoveDimBackgroundWindow()
        {
            UndimDialog();
        }
    }
}