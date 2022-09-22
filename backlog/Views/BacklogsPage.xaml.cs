using backlog.Auth;
using backlog.Logging;
using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using Google.Apis.Http;
using Microsoft.Graph;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BacklogsPage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<Backlog> Backlogs { get; set; }
        public ObservableCollection<Backlog> IncompleteBacklogs { get; set; }
        public ObservableCollection<Backlog> FilmBacklogs { get; set; }
        public ObservableCollection<Backlog> TvBacklogs { get; set; }
        public ObservableCollection<Backlog> GameBacklogs { get; set; }
        public ObservableCollection<Backlog> MusicBacklogs { get; set; }
        public ObservableCollection<Backlog> BookBacklogs { get; set; }

        private string _sortOrder = Settings.SortOrder;
        private bool _allEmpty;
        private bool _filmsEmpty;
        private bool _albumsEmpty;
        private bool _booksEmpty;
        private bool _tvEmpty;
        private bool _gamesEmpty;

        GraphServiceClient graphServiceClient;

        bool isNetworkAvailable = false;
        bool signedIn;
        int backlogIndex = -1;
        bool sync = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SortByName { get; }
        public ICommand SortByCreatedDateAsc { get; }
        public ICommand SortByCreatedDateDsc { get; }
        public ICommand SortByProgressAsc { get; }
        public ICommand SortByProgressDsc { get; }
        public ICommand SortByTargetDateAsc { get; }
        public ICommand SortByTargetDateDsc { get; }

        public string SortOrder
        {
            get => _sortOrder;
            set
            {
                if(value != _sortOrder)
                {
                    _sortOrder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SortOrder)));
                    Settings.SortOrder = _sortOrder;
                    PopulateBacklogs();
                }
            }
        }

        public bool BacklogsEmpty
        {
            get => _allEmpty;
            set
            {
                _allEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BacklogsEmpty)));
            }
        }

        public bool FilmsEmpty
        {
            get => _filmsEmpty;
            set
            {
                _filmsEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilmsEmpty)));
            }
        }

        public bool TVEmpty
        {
            get => _tvEmpty;
            set
            {
                _tvEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TVEmpty)));
            }
        }

        public bool BooksEmpty
        {
            get => _booksEmpty;
            set
            {
                _booksEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BooksEmpty)));
            }
        }

        public bool GamesEmpty
        {
            get => _gamesEmpty;
            set
            {
                _gamesEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GamesEmpty)));
            }
        }

        public bool AlbumsEmpty
        {
            get => _albumsEmpty;
            set
            {
                _albumsEmpty = value;
                Debug.WriteLine("Empty albums - ", value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlbumsEmpty)));
            }
        }

        public BacklogsPage()
        {
            this.InitializeComponent();

            SortByName = new Command(SortBacklogsByName);
            SortByCreatedDateAsc = new Command(SortBacklogsByCreatedDateAsc);
            SortByCreatedDateDsc = new Command(SortBacklogsByCreatedDateDsc);
            SortByProgressAsc = new Command(SortBacklogsByProgressAsc);
            SortByProgressDsc = new Command(SortBacklogsByProgressDsc);
            SortByTargetDateAsc = new Command(SortBacklogsByTargetDateAsc);
            SortByTargetDateDsc = new Command(SortBacklogsByTargetDateDsc);

            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            InitBacklogs();
            PopulateBacklogs();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null && e.Parameter.ToString() != "")
            {
                if (e.Parameter.ToString() == "sync")
                {
                    sync = true;
                }
                else
                {
                    // for backward connected animation
                    backlogIndex = int.Parse(e.Parameter.ToString());
                }
            }
            ProgBar.Visibility = Visibility.Visible;
            signedIn = Settings.IsSignedIn;
            if (isNetworkAvailable && signedIn)
            {
                if (sync)
                {
                    graphServiceClient = await MSAL.GetGraphServiceClient();
                    await SetUserPhotoAsync();
                    TopProfileButton.Visibility = Visibility.Visible;
                    BottomProfileButton.Visibility = Visibility.Visible;
                    try
                    {
                        await Logger.Info("Syncing backlogs....");
                    }
                    catch { }
                    //await SaveData.GetInstance().ReadDataAsync(true);
                    PopulateBacklogs();
                }
            }
            ProgBar.Visibility = Visibility.Collapsed;
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested;
        }

        /// <summary>
        /// Initalize backlogs
        /// </summary>
        private void InitBacklogs()
        {
            IncompleteBacklogs = SaveData.GetInstance().GetIncompleteBacklogs();
            FilmBacklogs = new ObservableCollection<Backlog>();
            TvBacklogs = new ObservableCollection<Backlog>();
            GameBacklogs = new ObservableCollection<Backlog>();
            MusicBacklogs = new ObservableCollection<Backlog>();
            BookBacklogs = new ObservableCollection<Backlog>();
        }

        private void View_BackRequested(object sender, BackRequestedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            catch
            {
                Frame.Navigate(typeof(MainPage));
            }
            e.Handled = true;
        }

        /// <summary>
        /// Populate the backlogs list with up-to-date backlogs
        /// </summary>
        private void PopulateBacklogs()
        {
            IncompleteBacklogs = SaveData.GetInstance().GetIncompleteBacklogs();
            ObservableCollection<Backlog> _backlogs = null;
            switch(SortOrder)
            {
                case "Name":
                    _backlogs = new ObservableCollection<Backlog>(IncompleteBacklogs.OrderBy(b => b.Name));
                    break;
                case "Created Date Asc.":
                    _backlogs = new ObservableCollection<Backlog>(IncompleteBacklogs.OrderBy(b => Convert.ToDateTime(b.CreatedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Created Date Dsc.":
                    _backlogs = new ObservableCollection<Backlog>(IncompleteBacklogs.OrderByDescending(b => Convert.ToDateTime(b.CreatedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Target Date Asc.":
                    _backlogs = new ObservableCollection<Backlog>(IncompleteBacklogs.OrderBy(b => Convert.ToDateTime(b.TargetDate, CultureInfo.InvariantCulture)));
                    break;
                case "Target Date Dsc.":
                    _backlogs = new ObservableCollection<Backlog>(IncompleteBacklogs.OrderByDescending(b => Convert.ToDateTime(b.TargetDate, CultureInfo.InvariantCulture)));
                    break;
                case "Progress Asc.":
                    _backlogs = new ObservableCollection<Backlog>(IncompleteBacklogs.OrderBy(b => b.Progress));
                    break;
                case "Progress Dsc.":
                    _backlogs = new ObservableCollection<Backlog>(IncompleteBacklogs.OrderByDescending(b => b.Progress));
                    break;
            }
            var _filmBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            var _tvBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            var _gameBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            var _musicBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Album.ToString()));
            var _bookBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Book.ToString()));
            IncompleteBacklogs.Clear();
            FilmBacklogs.Clear();
            TvBacklogs.Clear();
            GameBacklogs.Clear();
            MusicBacklogs.Clear();
            BookBacklogs.Clear();
            foreach (var b in _backlogs)
            {
                IncompleteBacklogs.Add(b);
            }
            foreach (var b in _bookBacklogs)
            {
                BookBacklogs.Add(b);
            }
            foreach (var b in _filmBacklogs)
            {
                FilmBacklogs.Add(b);
            }
            foreach (var b in _gameBacklogs)
            {
                GameBacklogs.Add(b);
            }
            foreach (var b in _tvBacklogs)
            {
                TvBacklogs.Add(b);
            }
            foreach (var b in _musicBacklogs)
            {
                MusicBacklogs.Add(b);
            }
            CheckEmptyBacklogs();
        }

        private void CheckEmptyBacklogs()
        {
            BacklogsEmpty = IncompleteBacklogs.Count <= 0;
            FilmsEmpty = FilmBacklogs.Count <= 0;
            BooksEmpty = BookBacklogs.Count <= 0;
            TVEmpty = TvBacklogs.Count <= 0;
            GamesEmpty = GameBacklogs.Count <= 0;
            AlbumsEmpty = MusicBacklogs.Count <= 0;
        }

        /// <summary>
        /// Set the user photo in the command bar
        /// </summary>
        /// <returns></returns>
        private async Task SetUserPhotoAsync()
        {
            string userName = Settings.UserName;
            TopProfileButton.Label = userName;
            BottomProfileButton.Label = userName;
            var cacheFolder = ApplicationData.Current.LocalCacheFolder;
            try
            {
                var accountPicFile = await cacheFolder.GetFileAsync("profile.png");
                using (IRandomAccessStream stream = await accountPicFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage image = new BitmapImage();
                    stream.Seek(0);
                    await image.SetSourceAsync(stream);
                    TopAccountPic.ProfilePicture = image;
                    BottomAccountPic.ProfilePicture = image;
                }
            }
            catch (Exception ex)
            {
                await Logger.Error("Error settings", ex);
            }
        }

        /// <summary>
        /// Opens the Backlog details page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BacklogView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            PivotItem pivotItem = (PivotItem)mainPivot.SelectedItem;
            // Prepare connected animation based on which section the user is on
            switch (pivotItem.Header.ToString())
            {
                default:
                    BacklogsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "films":
                    FilmsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "tv":
                    TVGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "books":
                    BooksGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "games":
                    GamesGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "albums":
                    AlbumsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
            }
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id, new SuppressNavigationTransitionInfo());
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
                Frame.Navigate(typeof(CreatePage), mainPivot.SelectedIndex, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
            }
            catch
            {
                Frame.Navigate(typeof(CreatePage), mainPivot.SelectedIndex);
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

        /// <summary>
        /// Sync backlogs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BacklogsPage), "sync");
        }

        private void CompletedBacklogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(CompletedBacklogsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            catch
            {
                Frame.Navigate(typeof(CompletedBacklogsPage));
            }
        }
        /// <summary>
        /// Finish connected animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BacklogsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (backlogIndex != -1)
            {
                ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("backAnimation");
                try
                {
                    await BacklogsGrid.TryStartConnectedAnimationAsync(animation, Backlogs[backlogIndex], "coverImage");
                }
                catch
                {
                    // : )
                }
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await SearchDialog.ShowAsync();
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
                        backlogsToSearch = new ObservableCollection<Backlog>(IncompleteBacklogs);
                        break;
                    case 1:
                        backlogsToSearch = new ObservableCollection<Backlog>(IncompleteBacklogs.Where(b => b.Type == "Film"));
                        break;
                    case 2:
                        backlogsToSearch = new ObservableCollection<Backlog>(IncompleteBacklogs.Where(b => b.Type == "Album"));
                        break;
                    case 3:
                        backlogsToSearch = new ObservableCollection<Backlog>(IncompleteBacklogs.Where(b => b.Type == "TV"));
                        break;
                    case 4:
                        backlogsToSearch = new ObservableCollection<Backlog>(IncompleteBacklogs.Where(b => b.Type == "Game"));
                        break;
                    case 5:
                        backlogsToSearch = new ObservableCollection<Backlog>(IncompleteBacklogs.Where(b => b.Type == "Book"));
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
            var selectedBacklog = IncompleteBacklogs.FirstOrDefault(b => b.Name == args.ChosenSuggestion.ToString());
            SearchDialog.Hide();
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id, null);
        }

        private async void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            await GenerateRandomBacklog();
        }

        /// <summary>
        /// Generates a random backlog of the selected type
        /// </summary>
        /// <returns></returns>
        private async Task GenerateRandomBacklog()
        {
            Backlog randomBacklog = new Backlog();
            Random random = new Random();
            bool error = false;
            switch (mainPivot.SelectedIndex)
            {
                case 0:
                    if (IncompleteBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = IncompleteBacklogs[random.Next(0, IncompleteBacklogs.Count)];
                    break;
                case 1:
                    if (FilmBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = FilmBacklogs[random.Next(0, FilmBacklogs.Count)];
                    break;
                case 2:
                    if (MusicBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = MusicBacklogs[random.Next(0, MusicBacklogs.Count)];
                    break;
                case 3:
                    if (TvBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = TvBacklogs[random.Next(0, TvBacklogs.Count)];
                    break;
                case 4:
                    if (GameBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = GameBacklogs[random.Next(0, GameBacklogs.Count)];
                    break;
                case 5:
                    if (BookBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = BookBacklogs[random.Next(0, BookBacklogs.Count)];
                    break;
            }

            if (!error)
            {
                await ShowRandomPick(randomBacklog);
            }
        }

        private async Task ShowErrorMessage(string message)
        {
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = "Not enough Backlogs",
                Content = message,
                CloseButtonText = "Ok"
            };
            await contentDialog.ShowAsync();
        }

        private async Task ShowRandomPick(Backlog backlog)
        {
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = "Your Pick",
                Content = $"Your current pick is {backlog.Name} by {backlog.Director}",
                CloseButtonText = "Ok",
                PrimaryButtonText = "Go again",
                SecondaryButtonText = "Open"
            };
            var result = await contentDialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                await GenerateRandomBacklog();
            }
            else if(result == ContentDialogResult.Secondary)
            {
                Frame.Navigate(typeof(BacklogPage), backlog.id, null);
            }
        }

        private void SortBacklogsByName()
        {
            SortOrder = "Name";
        }

        private void SortBacklogsByCreatedDateAsc()
        {
            SortOrder = "Created Date Asc.";
        }

        private void SortBacklogsByCreatedDateDsc()
        {
            SortOrder = "Created Date Dsc.";
        }

        private void SortBacklogsByTargetDateAsc()
        {
            SortOrder = "Target Date Asc.";
        }

        private void SortBacklogsByTargetDateDsc()
        {
            SortOrder = "Target Date Dsc.";
        }

        private void SortBacklogsByProgressAsc()
        {
            SortOrder = "Progress Asc.";
        }

        private void SortBacklogsByProgressDsc()
        {
            SortOrder = "Progress Dsc.";
        }
    }
}
