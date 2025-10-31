using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace oneKeyAi_win.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly Dictionary<string, Type> _pagesMap = new()
        {
            { "Home", typeof(Views.HomePage) },
            { "History", typeof(Views.HistoryPage) },
            { "Test", typeof(Views.TestPage) },
            { "Settings", typeof(Views.SettingsPage) }
        };

        // This method is kept for getting page types, but actual navigation is now handled in MainPage
        public Type GetPageTypeForTag(string tag)
        {
            return _pagesMap.TryGetValue(tag, out var pageType) ? pageType : typeof(Views.HistoryPage);
        }
    }
}