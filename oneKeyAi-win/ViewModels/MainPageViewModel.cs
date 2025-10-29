using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace oneKeyAi_win.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private object? _selectedPage;
        public object? SelectedPage
        {
            get => _selectedPage;
            set => SetProperty(ref _selectedPage, value);
        }

        private readonly Dictionary<string, Type> _pagesMap = new()
        {
            { "Home", typeof(Views.HomePage) },
            { "History", typeof(Views.HistoryPage) },
            { "Test", typeof(Views.TestPage) },
            { "Settings", typeof(Views.SettingsPage) }
        };

        public void NavigateToPage(string pageTag)
        {
            if (_pagesMap.TryGetValue(pageTag, out var pageType))
            {
                SelectedPage = Activator.CreateInstance(pageType);
            }
        }

        public Type GetPageTypeForTag(string tag)
        {
            return _pagesMap.TryGetValue(tag, out var pageType) ? pageType : typeof(Views.HistoryPage);
        }
    }
}