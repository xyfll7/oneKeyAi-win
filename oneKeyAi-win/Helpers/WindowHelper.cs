using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oneKeyAi_win.Helpers
{
    class WindowHelper
    {
        static public List<Window> ActiveWindows { get { return _activeWindows; } }

        static private List<Window> _activeWindows = new List<Window>();
        static public void TrackWindow(Window window)
        {
            window.Closed += (sender, args) =>
            {
                _activeWindows.Remove(window);
            };
            _activeWindows.Add(window);
        }
        static public void SetWindowMinSize(Window window, double width, double height)
        {
            if (window.Content is not FrameworkElement windowContent)
            {
                System.Diagnostics.Debug.WriteLine("Window content is not a FrameworkElement.");
                return;
            }

            if (windowContent.XamlRoot is null)
            {
                System.Diagnostics.Debug.WriteLine("Window content's XamlRoot is null.");
                return;
            }

            if (window.AppWindow.Presenter is not OverlappedPresenter presenter)
            {
                System.Diagnostics.Debug.WriteLine("Window's AppWindow.Presenter is not an OverlappedPresenter.");
                return;
            }

            var scale = windowContent.XamlRoot.RasterizationScale;
            var minWidth = width * scale;
            var minHeight = height * scale;
            presenter.PreferredMinimumWidth = (int)minWidth;
            presenter.PreferredMinimumHeight = (int)minHeight;
        }
    }
}
