using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using oneKeyAi_win.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace oneKeyAi_win.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel? _viewModel;

        public MainPage()
        {
            this.InitializeComponent();

            // 从XAML资源中获取ViewModel
            if (Resources.TryGetValue("MainPageViewModel", out object? value) && value is MainPageViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            // 设置NavigationView的导航事件
            NavigationView.ItemInvoked += OnNavigationViewItemInvoked;
            NavigateToPage("Home"); // 默认导航到首页
            SelectNavigationItem("Home");
        }

        private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                NavigateToPage("Settings");
            }
            else if (args.InvokedItemContainer is NavigationViewItem navItem)
            {
                string? navItemTag = navItem.Tag?.ToString();
                if (!string.IsNullOrEmpty(navItemTag))
                {
                    NavigateToPage(navItemTag);
                }
            }
        }

        private void NavigateToPage(string tag)
        {
            if (_viewModel != null)
            {
                Type pageType = _viewModel.GetPageTypeForTag(tag);
                NavigationViewFrame.Navigate(pageType);
            }
        }

        private void SelectNavigationItem(string tag)
        {
            foreach (NavigationViewItemBase item in NavigationView.MenuItems)
            {
                if (item is NavigationViewItem navViewItem && navViewItem.Tag?.ToString() == tag)
                {
                    NavigationView.SelectedItem = navViewItem;
                    return;
                }
            }
        }

        private void titleBar_PaneToggleRequested(TitleBar sender, object args)
        {

        }

        private void titleBar_BackRequested(TitleBar sender, object args)
        {

        }
    }
}
