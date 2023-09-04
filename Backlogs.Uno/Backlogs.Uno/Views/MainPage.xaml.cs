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

        private string clientId = "c81b068d-ab10-4c00-a24d-08c3a1a6b7c6"; // Use the client ID from Step 1
        private string redirectUri = $"ms-appx-web://microsoft.aad.brokerplugin/S-1-15-2-2566872105-1906516075-403359635-2971900813-1913047554-2806970718-2761120688"; // Use your app's actual redirect URI
        private string[] scopes = { "openid", "profile" }; // Customize the required scopes

        private IPublicClientApplication pca;
    

        //public MainViewModel? ViewModel { get; set; }
        //public MainPage()
        //{
        //    this.InitializeComponent();
        //    ViewModel = new MainViewModel(App.Services.GetRequiredService<INavigation>(), App.Services.GetRequiredService<IDialogHandler>(),
        //        App.Services.GetRequiredService<IShareDialogService>(), App.Services.GetRequiredService<IUserSettings>(),
        //        App.Services.GetRequiredService<IFileHandler>(),
        //        App.Services.GetRequiredService<IFilePicker>(), App.Services.GetRequiredService<IEmailService>(),
        //        App.Services.GetService<IMsal>(), App.Services.GetRequiredService<ISystemLauncher>());
        //}


        public MainPage()
        {
            this.InitializeComponent();



            pca = PublicClientApplicationBuilder
             .Create(clientId)
             .WithRedirectUri(redirectUri)
             .WithUnoHelpers()
             .Build();

        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var authResult = await pca.AcquireTokenInteractive(scopes)
                                 .WithUseEmbeddedWebView(true)
                                 .WithUnoHelpers()
                                 .ExecuteAsync();

                if (authResult != null)
                {
                    // Show the welcome message after authentication is successful
                    ShowWelcomeMessage(authResult.Account.Username);
                }
                else
                {
                    ShowMessage("Sign-in failed or canceled.");
                }
                ShowMessage("Sign-in successful");
                
            }
            catch (MsalException ex)
            {
                ShowMessage($"Sign-in failed: {ex.Message}");
            }
        }
        private void ShowWelcomeMessage(string? username)
        {
            //WelcomeTextBlock.Text = $"Welcome, {username}!";
           // Debug.WriteLine(username);
        }



private async void ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Authentication Result",
                Content = message,
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GotoCreatePage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreatePage));
        }

    }


}