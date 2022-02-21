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
using System.Globalization;
using System.Linq;

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
        string source;
        Uri sourceLink;
        PageStackEntry prevPage;
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
                    switch(backlog.Type)
                    {
                        case "Film":
                            source = "IMdB";
                            sourceLink = new Uri("https://www.imdb.com/");
                            break;
                        case "Album":
                            source = "LastFM";
                            sourceLink = new Uri("https://www.last.fm/");
                            break;
                        case "TV":
                            source = "IMdB";
                            sourceLink = new Uri("https://www.imbd.com");
                            break;
                        case "Game":
                            source = "IGDB";
                            sourceLink = new Uri("https://www.igdb.com/discover");
                            break;
                        case "Book":
                            source = "Google Books";
                            sourceLink = new Uri("https://books.google.com/");
                            break;
                    }
                    backlogIndex = backlogs.IndexOf(b);
                }
            }
            SourceLinkButton.Content = source;
            SourceLinkButton.NavigateUri = sourceLink;
            base.OnNavigatedTo(e);
            prevPage = Frame.BackStack.Last();
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
            Frame.Navigate(prevPage?.SourcePageType);
        }

        private void NumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            edited = true;
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
                Frame.Navigate(prevPage?.SourcePageType, backlogIndex, new SuppressNavigationTransitionInfo());
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
            await RatingDialog.ShowAsync();
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

        private async void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            await Logger.Info("Marking backlog as complete");
            backlog.IsComplete = true;
            backlog.UserRating = UserRating.Value;
            backlog.CompletedDate = DateTimeOffset.Now.Date.ToString("D", CultureInfo.InvariantCulture);
            await SaveBacklog();
            RatingDialog.Hide();
            Frame.Navigate(typeof(MainPage));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RatingDialog.Hide();
        }

        private void RatingSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            UserRating.Value = e.NewValue;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            TopDoneButton.Visibility = Visibility.Collapsed;
            TopEditButton.Visibility = Visibility.Collapsed;
            TopSaveButton.Visibility = Visibility.Visible;
            TopCancelButton.Visibility = Visibility.Visible;

            BottomDoneButton.Visibility = Visibility.Collapsed;
            BottomEditButton.Visibility = Visibility.Collapsed;
            BottomSaveButton.Visibility = Visibility.Visible;
            BottomCancelButton.Visibility = Visibility.Visible;

            CompletePanel.Visibility = Visibility.Collapsed;
            NotifPanel.Visibility = Visibility.Visible;
            DatesPanel.Visibility = Visibility.Collapsed;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DatePicker.SelectedDates.Count > 0)
            {
                if (TimePicker.Time == TimeSpan.Zero)
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Invalid date and time",
                        Content = "Please pick a time!",
                        CloseButtonText = "Ok"
                    };
                    await contentDialog.ShowAsync();
                    return;
                }
                string date = DatePicker.SelectedDates[0].ToString("D", CultureInfo.InvariantCulture);
                DateTimeOffset dateTime = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture).Add(TimePicker.Time);
                int diff = DateTimeOffset.Compare(dateTime, DateTimeOffset.Now);
                if (diff < 0)
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Invalid date and time",
                        Content = "The date and time you've chosen are in the past!",
                        CloseButtonText = "Ok"
                    };
                    await contentDialog.ShowAsync();
                    return;
                }
            }
            else if (TimePicker.Time != TimeSpan.Zero)
            {
                if (DatePicker.SelectedDates.Count <= 0)
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Invalid date and time",
                        Content = "Please pick a date!",
                        CloseButtonText = "Ok"
                    };
                    await contentDialog.ShowAsync();
                    return;
                }
            }
            ProgBar.Visibility = Visibility.Visible;
            backlog.TargetDate = DatePicker.SelectedDates[0].ToString("D", CultureInfo.InvariantCulture);
            backlog.NotifTime = TimePicker.Time;
            await SaveBacklog();
            CmdCancelButton_Click(sender, e);
            ProgBar.Visibility = Visibility.Collapsed;
        }

        private void CmdCancelButton_Click(object sender, RoutedEventArgs e)
        {
            TopDoneButton.Visibility = Visibility.Visible;
            TopEditButton.Visibility = Visibility.Visible;
            TopSaveButton.Visibility = Visibility.Collapsed;
            TopCancelButton.Visibility = Visibility.Collapsed;

            BottomDoneButton.Visibility = Visibility.Visible;
            BottomEditButton.Visibility = Visibility.Visible;
            BottomSaveButton.Visibility = Visibility.Collapsed;
            BottomCancelButton.Visibility = Visibility.Collapsed;

            CompletePanel.Visibility = Visibility.Visible;
            NotifPanel.Visibility = Visibility.Collapsed;
            DatesPanel.Visibility = Visibility.Visible;
        }
    }
}
