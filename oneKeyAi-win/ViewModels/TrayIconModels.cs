using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oneKeyAi_win.ViewModels
{
    public partial class TrayIconModels : ObservableObject
    {
        [RelayCommand]
        private void Test()
        {
            Console.WriteLine("Hello, World!");
        }
        [RelayCommand]
        private void ShowHideWindow()
        {
            // Logic to show/hide a window on demand
            Console.WriteLine("Hello, World!");
        }

        [RelayCommand]
        private void ExitApplication()
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
