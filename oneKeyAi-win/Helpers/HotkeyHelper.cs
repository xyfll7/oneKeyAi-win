using Microsoft.UI.Xaml.Input;
using NHotkey;
using NHotkey.WinUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using WindowsInput.Events;

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
                    VirtualKey.Y,
                    VirtualKeyModifiers.Control,
                    OnIncrement);
            }
            catch (HotkeyAlreadyRegisteredException)
            {
                // 热键被占用
                Debug.WriteLine("热键已被其他程序占用");
            }
        }
        private static async void OnIncrement(object? sender, HotkeyEventArgs e)
        {
            Debug.WriteLine("TODO: 增加逻辑");
    
            // TODO: 增加逻辑
            ClipboardHelper.OneKey();
            e.Handled = true;
        }
    }
}
