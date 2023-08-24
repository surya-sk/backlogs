﻿using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Core;
using Backlogs.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Backlogs.Services;
using System.Diagnostics;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Backlogs.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BacklogPage : Page
    {
        public BacklogViewModel ViewModel { get; set; }

        public BacklogPage()
        {
            this.InitializeComponent();
            this.Loading += BacklogPage_Loading;

            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
        }

        private async void BacklogPage_Loading(Windows.UI.Xaml.FrameworkElement sender, object args)
        {
            await ViewModel.GetDetailsAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Guid selectedId = (Guid)e.Parameter;
            ViewModel = new BacklogViewModel(selectedId, App.Services.GetRequiredService<INavigation>(), 
                App.Services.GetRequiredService<IDialogHandler>(), App.Services.GetRequiredService<IToastNotificationService>(), 
                App.Services.GetRequiredService<IShareDialogService>(), App.Services.GetRequiredService<IUserSettings>(),
                App.Services.GetRequiredService<IFileHandler>(), App.Services.GetRequiredService<ISystemLauncher>());
            base.OnNavigatedTo(e);
            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("cover");
            imageAnimation?.TryStart(img);
        }
    }
}
