using backlog.Logging;
using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompletedBacklogsPage : Page
    {
        private ObservableCollection<Backlog> FinishedBacklogs;
        private ObservableCollection<Backlog> FinishedFilmBacklogs;
        private ObservableCollection<Backlog> FinishedTVBacklogs;
        private ObservableCollection<Backlog> FinishedMusicBacklogs;
        private ObservableCollection<Backlog> FinishedGameBacklogs;
        private ObservableCollection<Backlog> FinishedBookBacklogs;
        private ObservableCollection<Backlog> Backlogs;
        private Backlog SelectedBacklog;
        private string sortOrder { get; set; }

        public CompletedBacklogsPage()
        {
            this.InitializeComponent();
            sortOrder = Settings.CompletedSortOrder;
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            Backlogs = SaveData.GetInstance().GetBacklogs();
            FinishedBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete));
            FinishedBookBacklogs = new ObservableCollection<Backlog>(FinishedBacklogs.Where(b => b.Type == BacklogType.Book.ToString()));
            FinishedFilmBacklogs = new ObservableCollection<Backlog>(FinishedBacklogs.Where(b => b.Type == BacklogType.Film.ToString()));
            FinishedGameBacklogs = new ObservableCollection<Backlog>(FinishedBacklogs.Where(b => b.Type == BacklogType.Game.ToString()));
            FinishedMusicBacklogs = new ObservableCollection<Backlog>(FinishedBacklogs.Where(b => b.Type == BacklogType.Album.ToString()));
            FinishedTVBacklogs = new ObservableCollection<Backlog>(FinishedBacklogs.Where(b => b.Type == BacklogType.TV.ToString()));
            if (FinishedBacklogs.Count < 1)
            {
                EmptyText.Visibility = Visibility.Visible;
                MainGrid.Visibility = Visibility.Collapsed; 
            }
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested;
        }

        private void View_BackRequested(object sender, BackRequestedEventArgs e)
        {
            PageStackEntry prevPage = Frame.BackStack.Last();
            try
            {
                Frame.Navigate(prevPage?.SourcePageType, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            catch
            {
                Frame.Navigate(prevPage?.SourcePageType);
            }
            e.Handled = true;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ProgRing.IsActive = true;
            foreach(var backlog in Backlogs)
            {
                if(backlog.id == SelectedBacklog.id)
                {
                    backlog.UserRating = PopupRating.Value;
                }
            }
            foreach (var backlog in FinishedBacklogs)
            {
                if (backlog.id == SelectedBacklog.id)
                {
                    backlog.UserRating = PopupRating.Value;
                }
            }
            SaveData.GetInstance().SaveSettings(Backlogs);
            await SaveData.GetInstance().WriteDataAsync(Settings.IsSignedIn);
            ProgRing.IsActive = false;
            CloseButton_Click(sender, e);
        }

        private async void IncompleteButton_Click(object sender, RoutedEventArgs e)
        {
            ProgRing.IsActive = true;
            foreach (var backlog in Backlogs)
            {
                if (backlog.id == SelectedBacklog.id)
                {
                    backlog.IsComplete = false;
                    backlog.CompletedDate = null;
                }
            }
            SaveData.GetInstance().SaveSettings(Backlogs);
            await SaveData.GetInstance().WriteDataAsync(Settings.IsSignedIn);
            PopupOverlay.Hide();
            Frame.Navigate(typeof(CompletedBacklogsPage));
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConnectedAnimation connectedAnimation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", destinationGrid);
                PopupOverlay.Hide();
                connectedAnimation.Configuration = new DirectConnectedAnimationConfiguration();
                await MainGrid.TryStartConnectedAnimationAsync(connectedAnimation, SelectedBacklog, "connectedElement");
            }
            catch
            {
                PopupOverlay.Hide();
            }
        }

        private async void MainGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = e.ClickedItem as Backlog;
            SelectedBacklog = selectedBacklog;
            try
            {
                ConnectedAnimation connectedAnimation = MainGrid.PrepareConnectedAnimation("forwardAnimation", SelectedBacklog, "connectedElement");
                connectedAnimation.Configuration = new DirectConnectedAnimationConfiguration();
                connectedAnimation.TryStart(destinationGrid);
            }
            catch (Exception ex)
            {
                await Logger.Error("Error with connected animation", ex);
                // ; )
            }

            PopupImage.Source = new BitmapImage(new Uri(selectedBacklog.ImageURL));
            PopupTitle.Text = selectedBacklog.Name;
            PopupDirector.Text = selectedBacklog.Director;
            PopupRating.Value = selectedBacklog.UserRating;
            PopupSlider.Value = selectedBacklog.UserRating;

            await PopupOverlay.ShowAsync();
        }

        private void PopupSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            PopupRating.Value = e.NewValue;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await SearchDialog.ShowAsync();
        }

        private void SortByName_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortByCompletedDateAsc_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortByCompletedDateDsc_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortByRatingAsc_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortByRatingDsc_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<string> suggestions = new List<string>();
                var splitText = sender.Text.ToLower().Split(' ');
                ObservableCollection<Backlog> backlogsToSearch = null;
                switch (mainPivot.SelectedIndex)
                {
                    case 0:
                        backlogsToSearch = new ObservableCollection<Backlog>(FinishedBacklogs);
                        break;
                    case 1:
                        backlogsToSearch = new ObservableCollection<Backlog>(FinishedFilmBacklogs);
                        break;
                    case 2:
                        backlogsToSearch = new ObservableCollection<Backlog>(FinishedMusicBacklogs);
                        break;
                    case 3:
                        backlogsToSearch = new ObservableCollection<Backlog>(FinishedTVBacklogs);
                        break;
                    case 4:
                        backlogsToSearch = new ObservableCollection<Backlog>(FinishedGameBacklogs);
                        break;
                    case 5:
                        backlogsToSearch = new ObservableCollection<Backlog>(FinishedBookBacklogs);
                        break;
                }
                foreach (var backlog in backlogsToSearch)
                {
                    var found = splitText.All((key) =>
                    {
                        return backlog.Name.ToLower().Contains(key);
                    });
                    if (found)
                    {
                        suggestions.Add(backlog.Name);
                    }
                }
                if (suggestions.Count == 0)
                {
                    suggestions.Add("No results found");
                }
                sender.ItemsSource = suggestions;
            }
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            mainPivot.SelectedIndex = 0;
            SearchDialog.Hide();
            var selectedBacklog = FinishedBacklogs.FirstOrDefault(b => b.Name == args.ChosenSuggestion.ToString());
            MainGrid.SelectedItem = selectedBacklog;
            MainGrid.ScrollIntoView(selectedBacklog);
        }
    }
}
