using backlog.Logging;
using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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
    public sealed partial class CompletedBacklogsPage : Page, INotifyPropertyChanged
    {

        private ICommand CloseBacklogPopup;

        private ObservableCollection<Backlog> FinishedBacklogs;
        private ObservableCollection<Backlog> FinishedFilmBacklogs;
        private ObservableCollection<Backlog> FinishedTVBacklogs;
        private ObservableCollection<Backlog> FinishedMusicBacklogs;
        private ObservableCollection<Backlog> FinishedGameBacklogs;
        private ObservableCollection<Backlog> FinishedBookBacklogs;
        private ObservableCollection<Backlog> Backlogs;
        private Backlog SelectedBacklog;
        private string sortOrder { get; set; }
        private bool _loading = false;

        public delegate Task CloseBacklogFunc();
        public delegate void ClosePopupFunc();
        public event PropertyChangedEventHandler PropertyChanged;

        public CloseBacklogFunc CloseBacklog;
        public ClosePopupFunc ClosePopup;

        public ICommand SaveBacklog { get; }
        public ICommand MarkBacklogAsIncomplete { get; }


        public bool IsLoading
        {
            get => _loading;
            set
            {
                _loading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }



        public CompletedBacklogsPage()
        {
            this.InitializeComponent();
            CloseBacklogPopup = new AsyncCommand(CloseBacklogAsync);


            SaveBacklog = new AsyncCommand(SaveBacklogAsync);
            MarkBacklogAsIncomplete = new AsyncCommand(MarkBacklogAsIncompleteAsync);
            CloseBacklog = CloseBacklogAsync;
            ClosePopup = ClosePopupOverlayAndReload;

            Backlogs = SaveData.GetInstance().GetBacklogs();
            FinishedBacklogs = new ObservableCollection<Backlog>();
            FinishedBookBacklogs = new ObservableCollection<Backlog>();
            FinishedFilmBacklogs = new ObservableCollection<Backlog>();
            FinishedTVBacklogs = new ObservableCollection<Backlog>();
            FinishedMusicBacklogs = new ObservableCollection<Backlog>();
            FinishedGameBacklogs = new ObservableCollection<Backlog>();
            PopulateBacklogs();
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested;
        }

        private void PopulateBacklogs()
        {
            sortOrder = Settings.CompletedSortOrder;
            TopSortButton.Label = sortOrder;
            BottomSortButton.Label = sortOrder;
            ObservableCollection<Backlog> _finishedBacklogs = null;
            switch(sortOrder)
            {
                case "Name":
                    _finishedBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete).OrderBy(b => b.Name));
                    break;
                case "Completed Date Asc.":
                    _finishedBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete).OrderBy(b => Convert.ToDateTime(b.CompletedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Completed Date Dsc.":
                    _finishedBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete).OrderByDescending(b => Convert.ToDateTime(b.CompletedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Lowest Rating":
                    _finishedBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete).OrderBy(b => b.UserRating));
                    break;
                case "Highest Rating":
                    _finishedBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete).OrderByDescending(b => b.UserRating));
                    break;

            }
            var _finishedBookBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Book.ToString()));
            var _finishedFilmBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Film.ToString()));
            var _finishedGameBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Game.ToString()));
            var _finishedMusicBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Album.ToString()));
            var _finishedTVBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.TV.ToString()));
            FinishedBacklogs.Clear();
            FinishedFilmBacklogs.Clear();
            FinishedTVBacklogs.Clear();
            FinishedMusicBacklogs.Clear();
            FinishedGameBacklogs.Clear();
            FinishedBookBacklogs.Clear();

            foreach(var backlog in _finishedBacklogs)
            {
                FinishedBacklogs.Add(backlog);
            }
            foreach (var backlog in _finishedBookBacklogs)
            {
                FinishedBookBacklogs.Add(backlog);
            }
            foreach (var backlog in _finishedFilmBacklogs)
            {
                FinishedFilmBacklogs.Add(backlog);
            }
            foreach (var backlog in _finishedGameBacklogs)
            {
                FinishedGameBacklogs.Add(backlog);
            }
            foreach (var backlog in _finishedMusicBacklogs)
            {
                FinishedMusicBacklogs.Add(backlog);
            }
            foreach (var backlog in _finishedTVBacklogs)
            {
                FinishedTVBacklogs.Add(backlog);
            }
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

        /// <summary>
        /// Saves the backlog
        /// </summary>
        /// <returns></returns>
        private async Task SaveBacklogAsync()
        {
            IsLoading = true;
            foreach (var backlog in Backlogs)
            {
                if (backlog.id == SelectedBacklog.id)
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
            IsLoading = false;
            Debug.WriteLine("Save complete");
            await CloseBacklog();
        }

        /// <summary>
        /// Marks backlog as incomplete
        /// </summary>
        /// <returns></returns>
        private async Task MarkBacklogAsIncompleteAsync()
        {
            IsLoading = true;
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
            ClosePopup();
        }

        private void ClosePopupOverlayAndReload()
        {
            PopupOverlay.Hide();
            Frame.Navigate(typeof(CompletedBacklogsPage));
        }

        /// <summary>
        /// Closes the backlog popup
        /// </summary>
        /// <returns></returns>
        private async Task CloseBacklogAsync()
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
            Settings.CompletedSortOrder = "Name";
            PopulateBacklogs();
        }

        private void SortByCompletedDateAsc_Click(object sender, RoutedEventArgs e)
        {
            Settings.CompletedSortOrder = "Completed Date Asc.";
            PopulateBacklogs();
        }

        private void SortByCompletedDateDsc_Click(object sender, RoutedEventArgs e)
        {
            Settings.CompletedSortOrder = "Completed Date Dsc.";
            PopulateBacklogs();
        }

        private void SortByRatingAsc_Click(object sender, RoutedEventArgs e)
        {
            Settings.CompletedSortOrder = "Lowest Rating";
            PopulateBacklogs();
        }

        private void SortByRatingDsc_Click(object sender, RoutedEventArgs e)
        {
            Settings.CompletedSortOrder = "Highest Rating";
            PopulateBacklogs();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
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
