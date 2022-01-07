using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using backlog.Models;
using backlog.Saving;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Animation;
using backlog.Logging;
using backlog.Utils;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BacklogPage : Page
    {
        private Backlog backlog;
        private ObservableCollection<Backlog> backlogs;
        private int backlogIndex;
        private bool edited;
        bool signedIn;
        public BacklogPage()
        {
            this.InitializeComponent();
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            backlogs = SaveData.GetInstance().GetBacklogs();
            signedIn = Settings.IsSignedIn;
            edited = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Guid selectedId = (Guid)e.Parameter;
            foreach (Backlog b in backlogs)
            {
                if (selectedId == b.id)
                {
                    backlog = b;
                    backlogIndex = backlogs.IndexOf(b);
                }
            }
            base.OnNavigatedTo(e);
            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("cover");
            imageAnimation?.TryStart(img);
        }

        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = backlog.Name,
                Content = backlog.Description,
                CloseButtonText = "Close"
            };
            await contentDialog.ShowAsync();
        }
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await Logger.Info("Deleting backlog.....");
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Delete backlog?",
                Content = "Deletion is permanent. This backlog cannot be recovered, and will be gone forever.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };
            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await DeleteConfirmation_Click();
            }
            await Logger.Info("Deleted backlog");
        }

        /// <summary>
        /// Delete a backlog after confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task DeleteConfirmation_Click()
        {
            ProgBar.Visibility = Visibility.Visible;
            backlogs.Remove(backlog);
            SaveData.GetInstance().SaveSettings(backlogs);
            await SaveData.GetInstance().WriteDataAsync(signedIn);
            Frame.Navigate(typeof(MainPage));
        }

        private void NumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            edited = true;
            BacklogProgressBar.Value = ProgressNumBox.Value;
        }

        private async void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backAnimation", img);
                animation.Configuration = new DirectConnectedAnimationConfiguration();
            }
            catch (Exception ex)
            {
                await Logger.Warn("Error occured during navigation:");
                await Logger.Trace(ex.StackTrace);
            }
            finally
            {
                Frame.Navigate(typeof(MainPage), backlogIndex, new SuppressNavigationTransitionInfo());
            }
            if (edited)
                await SaveBacklog();
        }

        /// <summary>
        /// Mark backlogs as completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            await Logger.Info("Marking backlog as complete");
            backlog.IsComplete = true;
            await SaveBacklog();
            Frame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// Write backlog to the json file locally and on OneDrive if signed-in
        /// </summary>
        /// <returns></returns>
        private async Task SaveBacklog()
        {
            await Logger.Info("Saving backlog....");
            backlogs[backlogIndex] = backlog;
            SaveData.GetInstance().SaveSettings(backlogs);
            await SaveData.GetInstance().WriteDataAsync(signedIn);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void RatingSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            UserRating.Value = RatingSlider.Value;
        }
    }
}
