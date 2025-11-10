using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TuteefyWPF.Classes
{
    internal class WindowHelper
    {
        public static void ShowDimmedDialog(Window parentWindow, Window dialog)
        {
            if (parentWindow == null || dialog == null)
                return;

            // Create overlay
            Grid overlay = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))
            };

            // Add overlay to main window content if it's a Grid
            if (parentWindow.Content is Grid mainGrid)
            {
                mainGrid.Children.Add(overlay);
            }

            double originalOpacity = parentWindow.Opacity;
            parentWindow.Opacity = 0.7;

            dialog.Owner = parentWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            dialog.Closed += (s, args) =>
            {
                parentWindow.Opacity = originalOpacity;

                if (parentWindow.Content is Grid grid)
                {
                    grid.Children.Remove(overlay);
                }
            };

            dialog.ShowDialog();
        }

        public static void UndimDialog(Window parentWindow)
        {
            if (parentWindow == null)
                return;

            // Restore full opacity
            parentWindow.Opacity = 1.0;

            // Remove any existing dim overlay
            if (parentWindow.Content is Grid grid)
            {
                foreach (var child in grid.Children.OfType<Grid>().ToList())
                {
                    if (child.Background is SolidColorBrush brush &&
                        brush.Color.A == 128 && brush.Color.R == 0 && brush.Color.G == 0 && brush.Color.B == 0)
                    {
                        grid.Children.Remove(child);
                        break;
                    }
                }
            }
        }
    }
}
