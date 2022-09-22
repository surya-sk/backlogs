using System;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using backlog.Utils;
using Windows.ApplicationModel.Email;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using backlog.Saving;
using backlog.Logging;
using System.Text;
using backlog.Auth;
using System.Windows.Input;
using Windows.UI.Xaml.Controls.Primitives;
using MvvmHelpers.Commands;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel.Channels;
using backlog.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; set; } = new SettingsViewModel();

        public SettingsPage()
        {
            this.InitializeComponent();
            ViewModel.NavigateToMainPageFunc = NavigateToMainPage;
            // show back button
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter != null)
            {
                mainPivot.SelectedIndex = (int)e.Parameter;
            }
            if(ViewModel.SignedIn)
            {
                await ViewModel.SetUserPhotoAsync();
            }
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Go back if possible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }

            e.Handled = true;
        }

        private void NavigateToMainPage()
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
