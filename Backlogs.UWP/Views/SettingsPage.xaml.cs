using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Backlogs.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Backlogs.Services;
using System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Backlogs.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; set; }

        public SettingsPage()
        {
            this.InitializeComponent();

            ViewModel = new SettingsViewModel(App.Services.GetRequiredService<INavigation>(), App.Services.GetRequiredService<IDialogHandler>(),
                App.Services.GetRequiredService<IFileHandler>(), App.Services.GetRequiredService<IEmailService>(),
                App.Services.GetRequiredService<IUserSettings>(), App.Services.GetService<IMsal>());
            // show back button
            this.DataContext = ViewModel;
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested;
        }

        private void View_BackRequested(object sender, BackRequestedEventArgs e)
        {
            ViewModel.GoBack();
            e.Handled = true;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter != null)
            {
                var _ = (string)e.Parameter;
                int sectionIndex = int.Parse(_);
                mainHub.ScrollToSection(mainHub.Sections[sectionIndex]);
            }
            await ViewModel.SetUserPhotoAsync();
            base.OnNavigatedTo(e);
        }
    }
}
