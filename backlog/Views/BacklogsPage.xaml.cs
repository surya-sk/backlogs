using backlog.Auth;
using backlog.Logging;
using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class BacklogsPage : Page
    {
        private ObservableCollection<Backlog> allBacklogs { get; set; }
        private ObservableCollection<Backlog> backlogs { get; set; }
        private ObservableCollection<Backlog> filmBacklogs { get; set; }
        private ObservableCollection<Backlog> tvBacklogs { get; set; }
        private ObservableCollection<Backlog> gameBacklogs { get; set; }
        private ObservableCollection<Backlog> musicBacklogs { get; set; }
        private ObservableCollection<Backlog> bookBacklogs { get; set; }
        GraphServiceClient graphServiceClient;

        string sortOrder { get; set; }

        bool isNetworkAvailable = false;
        bool signedIn;
        int backlogIndex = -1;
        bool sync = false;
        public BacklogsPage()
        {
            this.InitializeComponent();
            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            sortOrder = Settings.SortOrder;
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            InitBacklogs();
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
                graphServiceClient = await MSAL.GetGraphServiceClient();
                await SetUserPhotoAsync();
                TopProfileButton.Visibility = Visibility.Visible;
                BottomProfileButton.Visibility = Visibility.Visible;
                if (sync)
                {
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
        }

        /// <summary>
        /// Initalize backlogs
        /// </summary>
        private void InitBacklogs()
        {
            allBacklogs = SaveData.GetInstance().GetBacklogs();
            var readBacklogs = new ObservableCollection<Backlog>(allBacklogs.Where(b => b.IsComplete == false));
            backlogs = new ObservableCollection<Backlog>(readBacklogs.OrderBy(b => b.CreatedDate));
            filmBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            tvBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            gameBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            musicBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Album.ToString()));
            bookBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Book.ToString()));
            ShowEmptyMessage();
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested;
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
            var readBacklogs = SaveData.GetInstance().GetBacklogs().Where(b => b.IsComplete == false);
            var _backlogs = new ObservableCollection<Backlog>(readBacklogs.OrderBy(b => b.CreatedDate)); // sort by last created
            var _filmBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            var _tvBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            var _gameBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            var _musicBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Album.ToString()));
            var _bookBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Book.ToString()));
            backlogs.Clear();
            filmBacklogs.Clear();
            tvBacklogs.Clear();
            gameBacklogs.Clear();
            musicBacklogs.Clear();
            bookBacklogs.Clear();
            EmptyListText.Visibility = Visibility.Collapsed;
            foreach (var b in _backlogs)
            {
                backlogs.Add(b);
            }
            foreach (var b in _bookBacklogs)
            {
                bookBacklogs.Add(b);
            }
            foreach (var b in _filmBacklogs)
            {
                filmBacklogs.Add(b);
            }
            foreach (var b in _gameBacklogs)
            {
                gameBacklogs.Add(b);
            }
            foreach (var b in _tvBacklogs)
            {
                tvBacklogs.Add(b);
            }
            foreach (var b in _musicBacklogs)
            {
                musicBacklogs.Add(b);
            }
            ShowEmptyMessage();
        }

        private void ShowEmptyMessage()
        {
            ObservableCollection<Backlog>[] _backlogs = { backlogs, filmBacklogs, tvBacklogs, gameBacklogs, musicBacklogs, bookBacklogs };
            TextBlock[] textBlocks = { EmptyListText, EmptyFilmsText, EmptyTVText, EmptyGamesText, EmptyMusicText, EmptyBooksText };
            for (int i = 0; i < _backlogs.Length; i++)
            {
                if (_backlogs[i].Count <= 0)
                {
                    textBlocks[i].Visibility = Visibility.Visible;
                    if (i > 0)
                    {
                        textBlocks[i].Text = $"Nothing to see here. Add some!";
                    }
                }
                else
                {
                    textBlocks[i].Visibility = Visibility.Collapsed;
                }
            }
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
                    await BacklogsGrid.TryStartConnectedAnimationAsync(animation, allBacklogs[backlogIndex], "coverImage");
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
                foreach (var backlog in backlogs)
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
            var selectedBacklog = backlogs.FirstOrDefault(b => b.Name == args.ChosenSuggestion.ToString());
            SearchDialog.Hide();
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id, null);
        }

        private async void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            await GenerateRandomBacklog();

        }

        private async Task GenerateRandomBacklog()
        {
            Backlog randomBacklog = new Backlog();
            Random random = new Random();
            bool error = false;
            switch (mainPivot.SelectedIndex)
            {
                case 0:
                    if (backlogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = backlogs[random.Next(0, backlogs.Count)];
                    break;
                case 1:
                    if (filmBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = filmBacklogs[random.Next(0, filmBacklogs.Count)];
                    break;
                case 2:
                    if (musicBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = musicBacklogs[random.Next(0, musicBacklogs.Count)];
                    break;
                case 3:
                    if (tvBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = tvBacklogs[random.Next(0, tvBacklogs.Count)];
                    break;
                case 4:
                    if (gameBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = gameBacklogs[random.Next(0, gameBacklogs.Count)];
                    break;
                case 5:
                    if (bookBacklogs.Count < 2)
                    {
                        await ShowErrorMessage("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = bookBacklogs[random.Next(0, bookBacklogs.Count)];
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

        private void SortByName_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortByCreatedDateAsc_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortByCreatedDateDsc_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortByTargetDateAsc_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortByTargetDateDsc_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortByProgressAsc_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortByProgressDsc_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
