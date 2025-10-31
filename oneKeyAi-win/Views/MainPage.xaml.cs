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
        private readonly MainPageViewModel? _viewModel;

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
            
            // 设置Frame的导航事件，用于在页面导航时更新选中的NavigationViewItem
            NavigationViewFrame.Navigated += (_,_) => OnFrameNavigated();

            NavigateToPage("Home"); // 默认导航到首页
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

        // 新的导航方法，直接使用Frame进行导航
        public void NavigateToPage(string pageTag)
        {
            var pageType = _viewModel?.GetPageTypeForTag(pageTag);
            if (pageType != null)
            {
                NavigationViewFrame.Navigate(pageType);
            }
        }

        private void OnFrameNavigated()
        {
            // 根据当前页面类型更新NavigationView的选中项 - 不依赖参数e
            if (NavigationViewFrame.Content != null)
            {
                Type currentPageType = NavigationViewFrame.Content.GetType();
                var pagesMap = _viewModel?.GetPagesMap();
                if (pagesMap != null)
                {
                    // Create reverse mapping to find the tag for the current page type
                    var reverseMap = new Dictionary<Type, string>();
                    foreach (var kvp in pagesMap)
                    {
                        reverseMap[kvp.Value] = kvp.Key;
                    }

                    if (reverseMap.TryGetValue(currentPageType, out string? pageTag) && pageTag != null)
                    {
                        foreach (NavigationViewItemBase item in NavigationView.MenuItems.Cast<NavigationViewItemBase>())
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

        private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            NavigationView.IsPaneOpen = !NavigationView.IsPaneOpen;
        }

        private void TitleBar_BackRequested(TitleBar sender, object args)
        {
            if (NavigationViewFrame.CanGoBack)
            {
                NavigationViewFrame.GoBack();
            }
        }
    }
}
