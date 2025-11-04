using Microsoft.UI.Xaml.Input;
using NHotkey;
using NHotkey.WinUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace oneKeyAi_win.Helpers
{
    internal class HotkeyHelper
    {
        public static void RegisterGlobalHotkeys()
        {
            try
            {
                HotkeyManager.Current.AddOrReplace(
                    "Increment",
                    VirtualKey.Up,
                    VirtualKeyModifiers.Control,
                    OnIncrement);

                HotkeyManager.Current.AddOrReplace(
                    "Decrement",
                    VirtualKey.Down,
                    VirtualKeyModifiers.Control,
                    OnDecrement);
            }
            catch (HotkeyAlreadyRegisteredException)
            {
                // 热键被占用
                System.Diagnostics.Debug.WriteLine("热键已被其他程序占用");
            }
        }
        private static void OnIncrement(object? sender, HotkeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("TODO: 增加逻辑");
            // TODO: 增加逻辑
            e.Handled = true;
        }

        private static void OnDecrement(object? sender, HotkeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("TODO: 减少逻辑");
            // TODO: 减少逻辑
            e.Handled = true;
        }

    }
}
