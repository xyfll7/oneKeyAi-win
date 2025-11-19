using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using oneKeyAi_win.Helpers;
using oneKeyAi_win.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace oneKeyAi_win
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly MainPageViewModel? _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
            this.AppWindow.SetIcon("Assets/Logo.ico");
            //this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

            // Initialize the ViewModel directly since Resources is not available in Window
            _viewModel = new MainPageViewModel();

            NavigationView.ItemInvoked += OnNavigationViewItemInvoked;
           
            NavigationViewFrame.Navigated += (_, _) => OnFrameNavigated();

            NavigateToPage("Home"); 
            OnFrameNavigated();
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

        public void NavigateToPage(string pageTag)
        {
            var pageType = _viewModel?.GetPageTypeForTag(pageTag);
            if (pageType != null)
            {
                if (NavigationViewFrame.Content != null && NavigationViewFrame.Content.GetType() == pageType)
                {
                    return;
                }

                NavigationViewFrame.Navigate(pageType);
            }
        }

        private void OnFrameNavigated()
        {
            if (NavigationViewFrame.Content != null)
            {
                Type currentPageType = NavigationViewFrame.Content.GetType();
                var pagesMap = _viewModel?.GetPagesMap();
                if (pagesMap != null)
                {
                    var reverseMap = new Dictionary<Type, string>();
                    foreach (var kvp in pagesMap)
                    {
                        reverseMap[kvp.Value] = kvp.Key;
                    }

                    if (reverseMap.TryGetValue(currentPageType, out string? pageTag) && pageTag != null)
                    {
                        foreach (NavigationViewItemBase item in NavigationView.MenuItems.OfType<NavigationViewItemBase>())
                        {
                            if (item is NavigationViewItem navViewItem && navViewItem.Tag?.ToString() == pageTag)
                            {
                                NavigationView.SelectedItem = navViewItem;
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void AppTitleBar_PaneToggleRequested(TitleBar _, object __)
        {
            NavigationView.IsPaneOpen = !NavigationView.IsPaneOpen;
        }

        private void AppTitleBar_BackRequested(TitleBar _, object __)
        {
            if (NavigationViewFrame.CanGoBack)
            {
                NavigationViewFrame.GoBack();
            }
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs _)
        {
            WindowHelper.SetWindowMinSize(this, 640, 500);

            if (sender is FrameworkElement rootGrid && rootGrid.XamlRoot is not null)
            {
                rootGrid.XamlRoot.Changed += (_,_) => WindowHelper.SetWindowMinSize(this, 640, 500);
            }
        }
    }
}
