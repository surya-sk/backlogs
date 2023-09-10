using Backlogs.Services;
using Backlogs.ViewModels;
using Microsoft.Identity.Client;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;
using Uno.UI.MSAL;
using System.Diagnostics;

namespace Backlogs.Uno.Views
{
    public sealed partial class MainPage : Page
    {   
        public MainViewModel? ViewModel { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel(App.Services.GetRequiredService<INavigation>(), App.Services.GetRequiredService<IDialogHandler>(),
                App.Services.GetRequiredService<IShareDialogService>(), App.Services.GetRequiredService<IUserSettings>(),
                App.Services.GetRequiredService<IFileHandler>(),
                App.Services.GetRequiredService<IFilePicker>(), App.Services.GetRequiredService<IEmailService>(),
                App.Services.GetService<IMsal>(), App.Services.GetRequiredService<ISystemLauncher>());
        }

        private void GotoCreatePage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreatePage));
        }

    }


}