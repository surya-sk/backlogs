using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using backlog.Utils;
using System.Linq;
using Windows.UI.Core;
using Windows.System.Profile;
using System.Windows.Input;
using MvvmHelpers.Commands;
using backlog.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BacklogPage : Page
    {
        PageStackEntry prevPage;

        public ICommand CloseWebViewTrailer { get; }

        public BacklogViewModel ViewModel { get; set; }


        public BacklogPage()
        {
            this.InitializeComponent();

            CloseWebViewTrailer = new Command(CloseWebView);
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Guid selectedId = (Guid)e.Parameter;
            ViewModel = new BacklogViewModel(selectedId);
            ViewModel.LaunchWebViewFunc = LaunchTrailerWebView;
            ViewModel.NavigateToPreviousPageFunc = NavigateToPrevPageCallback;
            ViewModel.ShowRatingPopupFunc = ShowRatingDialogCallbackAsync;
            ViewModel.CloseRatingPopupFunc = CloseRatingDialogCallback;
            base.OnNavigatedTo(e);
            prevPage = Frame.BackStack.Last();
            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("cover");
            imageAnimation?.TryStart(img);
        }

        private void NavigateToPrevPageCallback()
        {
            Frame.Navigate(prevPage?.SourcePageType);
        }

        private async Task ShowRatingDialogCallbackAsync()
        {
            await RatingDialog.ShowAsync();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void CloseRatingDialogCallback()
        {
            RatingDialog.Hide();
        }

        private async Task LaunchTrailerWebView(string video)
        {
            await trailerDialog.ShowAsync();
            try
            {
                trailerDialog.CornerRadius = new CornerRadius(0); // Without this, for some fucking reason, buttons inside the WebView do not work
            }
            catch
            {

            }
            string width = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile" ? "600" : "500";
            string height = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile" ? "100%" : "400";
            webView.NavigateToString($"<iframe width=\"{width}\" height=\"{height}\" src=\"https://www.youtube.com/embed/{video}?autoplay={Settings.AutoplayVideos}\" title=\"YouTube video player\"  allow=\"accelerometer; autoplay; encrypted-media; gyroscope;\"></iframe>");

        }

        /// <summary>
        /// Navigate to a blank page because audio keeps playing after closing the WebView for some reason
        /// </summary>
        private void CloseWebView()
        {
            webView.Navigate(new Uri("about:blank"));
        }
    }
}
