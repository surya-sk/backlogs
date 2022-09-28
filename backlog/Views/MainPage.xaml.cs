using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using backlog.Models;
using backlog.Saving;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;
using backlog.Logging;
using backlog.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int backlogIndex = -1;

        public MainViewModel ViewModel { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel();
            ViewModel.ShowLastCrashLogFunc = ShowCrashLogCallback;
            ViewModel.ReloadAndSyncFunc = ReloadAndSyncCallback;
            ViewModel.OpenImportPageFunc = OpenImportPageCallback;
            
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
        }


        private async Task ShowCrashLogCallback(string log)
        {
            CrashDialog.Content = $"It seems the application crashed the last time, with the following error: {log}";
            await CrashDialog.ShowAsync();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null && e.Parameter.ToString() != "")
            {
                if (e.Parameter.ToString() == "sync")
                {
                    ViewModel.Sync = true;
                    try
                    {
                        await Logger.Info("Syncing backlogs");
                    }
                    catch { }
                }
                else
                {
                    // for backward connected animation
                    backlogIndex = int.Parse(e.Parameter.ToString());
                }
            }
            await ViewModel.SetupProfile();
        }


        private void ReloadAndSyncCallback()
        {
            Frame.Navigate(typeof(MainPage), "sync");
        }

        /// <summary>
        /// Opens the Create page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(CreatePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom});
            }
            catch
            {
                Frame.Navigate(typeof(CreatePage));
            }
        }

        /// <summary>
        /// Opens the Setting page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void CompletedBacklogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(CompletedBacklogsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight});
            }
            catch
            {
                Frame.Navigate(typeof(CompletedBacklogsPage));
            }
        }

        private void BacklogsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BacklogsPage));
        }

        private void AddedBacklogsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            AddedBacklogsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id, new SuppressNavigationTransitionInfo());
        }

        private async void AddedBacklogsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (backlogIndex != -1)
            {
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("backAnimation");
                try
                {
                    await AddedBacklogsGrid.TryStartConnectedAnimationAsync(animation, SaveData.GetInstance().GetBacklogs()[backlogIndex], "coverImage");
                }
                catch
                {
                    // : )
                }
            }
        }

        private void UpcomingBacklogsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            UpcomingBacklogsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id, new SuppressNavigationTransitionInfo());
        }

        private async void UpcomingBacklogsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if(backlogIndex != -1)
            {
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("backAnimation");
                try
                {
                    await UpcomingBacklogsGrid.TryStartConnectedAnimationAsync(animation, SaveData.GetInstance().GetBacklogs()[backlogIndex], "coveerImage");
                }
                catch
                {
                    // : )
                }
            }
        }

        private void InProgressBacklogsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            InProgressBacklogsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id, new SuppressNavigationTransitionInfo());
        }

        private async void InProgressBacklogsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (backlogIndex != -1)
            {
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("backAnimation");
                try
                {
                    await InProgressBacklogsGrid.TryStartConnectedAnimationAsync(animation, SaveData.GetInstance().GetBacklogs()[backlogIndex], "coverImage");
                }
                catch
                {
                    // : )
                }
            }
        }

        private void AllAddedButton_Click(object sender, RoutedEventArgs e)
        {
            BacklogsButton_Click(sender, e);
        }

        private void AllCompletedButton_Click(object sender, RoutedEventArgs e)
        {
            CompletedBacklogsButton_Click(sender, e);
        }

        private void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Frame.Navigate(typeof(BacklogPage), ViewModel.RandomBacklogId, null);
        }

        private void OpenImportPageCallback(string filename)
        {
            Frame.Navigate(typeof(ImportBacklog), filename, null);
        }

        private void WhatsNewTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            Frame.Navigate(typeof(SettingsPage), 1);
        }
    }
}
