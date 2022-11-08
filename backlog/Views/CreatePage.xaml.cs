using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using backlog.Logging;
using Windows.UI.Xaml.Media.Animation;
using System.Linq;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using System.Windows.Input;
using MvvmHelpers.Commands;
using System.ComponentModel;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreatePage : Page, INotifyPropertyChanged
    {
        private string m_selectedType;
        private int m_selectedIndex;
        private string m_placeholderText = "Enter name";
        private string m_nameInput;
        private DateTimeOffset m_dateInput;
        private TimeSpan m_notifTime;
        private bool m_enableNotificationToggle;
        private bool m_showNotificationToggle;
        private bool m_showNotificationOptions;
        private Models.SearchResult m_selectedResult;
        private string m_searchResultTitle;

        public ObservableCollection<Models.SearchResult> SearchResults;

        public ICommand SearchBacklog { get; }
        public ICommand Cancel { get; }

        public delegate void CancelCreate();

        public CancelCreate CancelCreateFunc;

        public string SelectedType
        {
            get => m_selectedType;
            set
            {
                if(m_selectedType != value)
                {
                    m_selectedType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedType)));
                    SetPlaceholderText(m_selectedType);
                }
            }
        }

        public string PlaceholderText
        {
            get => m_placeholderText;
            set
            {
                m_placeholderText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaceholderText)));
            }
        }

        public string NameInput
        {
            get => m_nameInput;
            set
            {
                m_nameInput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameInput)));
            }
        }

        public int SelectedIndex
        {
            get => m_selectedIndex;
            set
            {
                m_selectedIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
            }
        }

        public DateTimeOffset DateInput
        {
            get => m_dateInput;
            set
            {
                m_dateInput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DateInput)));
                EnableNotificationToggle = m_dateInput != DateTimeOffset.MinValue;
            }
        }

        public TimeSpan NotifTime
        {
            get => m_notifTime;
            set
            {
                if(m_notifTime != value)
                {
                    m_notifTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotifTime)));
                }
            }
        }

        public bool EnableNotificationToggle
        {
            get => m_enableNotificationToggle;
            set
            {
                m_enableNotificationToggle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableNotificationToggle)));
            }
        }

        public bool ShowNotificationToggle
        {
            get => m_showNotificationToggle;
            set
            {
                m_showNotificationToggle = value;
                if (m_showNotificationToggle)
                {
                    ShowNotificationOptions = true;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowNotificationToggle)));
            }
        }

        public bool ShowNotificationOptions
        {
            get => m_showNotificationOptions;
            set
            {
                m_showNotificationOptions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowNotificationOptions)));
            }
        }

        public Models.SearchResult SelectedSearchResult
        {
            get => m_selectedResult;
            set
            {
                m_selectedResult = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSearchResult)));
                SearchResultSelectedAsync().Wait();
            }
        }

        public string SearchResultTitle
        {
            get => m_searchResultTitle;
            set
            {
                m_searchResultTitle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchResultTitle)));
            }
        }

        ObservableCollection<Backlog> backlogs { get; set; }
        bool signedIn;
        bool isNetworkAvailable = false;
        int typeIndex = 0;
        DateTime today = DateTime.Today;

        public event PropertyChangedEventHandler PropertyChanged;

        public CreatePage()
        {
            this.InitializeComponent();
            SearchResults = new ObservableCollection<Models.SearchResult>();
            SearchBacklog = new AsyncCommand(TrySearchBacklogAsync);
            Cancel = new Command(CancelCreation);

            CancelCreateFunc = CancelCreateAndGoBack;

            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            signedIn = Settings.IsSignedIn;
            if(e.Parameter != null)
            {
                typeIndex = (int)e.Parameter;
                if(typeIndex > 0)
                {
                    SelectedIndex = typeIndex-1;
                }
            }
            if(isNetworkAvailable)
            {
                await Logger.Info("Fetching backlogs...");
                if(signedIn)
                {
                    await SaveData.GetInstance().ReadDataAsync(true);
                    backlogs = SaveData.GetInstance().GetBacklogs();
                }
                else
                {
                    await SaveData.GetInstance().ReadDataAsync();
                    backlogs = SaveData.GetInstance().GetBacklogs();
                }
            }
           else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "No internet",
                    Content = "You need the internet to create backlogs",
                    CloseButtonText = "Ok"
                };
                await Logger.Warn("Not connected to the internet");
                await contentDialog.ShowAsync();
            }
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Show hint text according to the selected type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetPlaceholderText(string value)
        {
            switch (value)
            {
                case "Film":
                    PlaceholderText = "Evil Dead";
                    break;
                case "TV":
                    PlaceholderText = "Hannibal";
                    break;
                case "Album":
                    PlaceholderText = "Radiohead - Amnesiac";
                    break;
                case "Game":
                    PlaceholderText = "Assassins Creed 2";
                    break;
                case "Book":
                    PlaceholderText = "Never Let Me Go";
                    break;
            }
        }


        /// <summary>
        /// Try and search for backlog
        /// </summary>
        /// <returns></returns>
        private async Task TrySearchBacklogAsync()
        {
            try
            {
                await Logger.Info("Creating backlog.....");
            }
            catch (Exception ex)
            {
                await Logger.Error("Error", ex);
            }

            try
            {
                if (NameInput == "" || SelectedIndex < 0)
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Missing fields",
                        Content = "Please fill in all the values",
                        CloseButtonText = "Ok"
                    };
                    await contentDialog.ShowAsync();
                }
                else
                {
                    if (DateInput != null && DateInput != DateTimeOffset.MinValue)
                    {
                        string date = DateInput.DateTime.ToString("D", CultureInfo.InvariantCulture);
                        if (ShowNotificationOptions)
                        {
                            if (NotifTime == TimeSpan.Zero)
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
                            DateTimeOffset dateTime = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture).Add(NotifTime);
                            int diff = DateTimeOffset.Compare(dateTime, DateTimeOffset.Now);
                            if (diff < 0)
                            {
                                ContentDialog contentDialog = new ContentDialog
                                {
                                    Title = "Invalid time",
                                    Content = "The date and time you've chosen are in the past!",
                                    CloseButtonText = "Ok"
                                };
                                await contentDialog.ShowAsync();
                                return;
                            }
                        }
                        else
                        {
                            DateTimeOffset dateTime = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture);
                            int diff = DateTime.Compare(DateTime.Today, DateInput.DateTime);
                            if (diff > 0)
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
                    }
                    SearchResultTitle = $"Showing results for \"{NameInput}\". Click the one you'd like to add";
                    await SearchBacklogAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                await Logger.Error("Failed to create backlog", ex);
            }
        }

        /// <summary>
        /// Search for the backlog metadata
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private async Task SearchBacklogAsync()
        {
            ProgBar.Visibility = Visibility.Visible;
            switch (SelectedType)
            {
                case "Film":
                    await SearchFilmBacklogAsync();
                    break;
                case "TV":
                    await SearchSeriesBacklogAsync();
                    break;
                case "Game":
                    await SearchGameBacklogAsync();
                    break;
                case "Book":
                    await SearchBookBacklogAsync();
                    break;
                case "Album":
                    await CreateMusicBacklogAsync();
                    break;
            }
            ProgBar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Creates a backlog item
        /// </summary>
        /// <param name="backlog"></param>
        /// <returns></returns>
        private async Task CreateBacklogItemAsync(Backlog backlog)
        {
            ProgBar.Visibility = Visibility.Visible;
            if (backlog != null)
            {
                backlogs.Add(backlog);
                SaveData.GetInstance().SaveSettings(backlogs);
                if (backlog.TargetDate != "None" && backlog.NotifTime != TimeSpan.Zero)
                {
                    var notifTime = DateTimeOffset.Parse(backlog.TargetDate, CultureInfo.InvariantCulture).Add(backlog.NotifTime);
                    var builder = new ToastContentBuilder()
                        .AddText($"It's {backlog.Name} time!")
                        .AddText($"You wanted to check out {backlog.Name} by {backlog.Director} today. Get to it!")
                        .AddHeroImage(new Uri(backlog.ImageURL));
                    ScheduledToastNotification toastNotification = new ScheduledToastNotification(builder.GetXml(), notifTime);
                    ToastNotificationManager.CreateToastNotifier().AddToSchedule(toastNotification);
                }
                await SaveData.GetInstance().WriteDataAsync(signedIn);
                PageStackEntry prevPage = Frame.BackStack.Last();
                try
                {
                    Frame.Navigate(prevPage?.SourcePageType, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                }
                catch
                {
                    Frame.Navigate(prevPage?.SourcePageType);
                }
            }
            else
            {
                await ShowErrorDialogAsync();
                ProgBar.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Searches for a film
        /// </summary>
        /// <returns></returns>
        private async Task SearchFilmBacklogAsync()
        {
            try
            {
                string response = await RestClient.GetFilmResponse(NameInput);
                await Logger.Info($"Trying to find film {NameInput}. Response: {response}");
                FilmResult filmResult = JsonConvert.DeserializeObject<FilmResult>(response);
                if(filmResult.results.Length > 0)
                {
                    SearchResults.Clear();
                    foreach (var result in filmResult.results)
                    {
                        try
                        {
                            SearchResults.Add(new Models.SearchResult
                            {
                                Id = result.id,
                                Name = result.title,
                                Description = result.description,
                                ImageURL = result.image
                            });
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    _ = await ResultsDialog.ShowAsync();
                    await Logger.Info("Succesfully created backlog");
                }
                else
                {
                    await ShowNotFoundDialogAsync();
                }
            }
            catch (Exception e)
            {
                await Logger.Error("Failed to find film.", e);
            }
        }

        /// <summary>
        /// Create a film backlog
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private async Task CreateFilmBacklogAsync(string date)
        {
            string filmData = await RestClient.GetFilmDataResponse(SelectedSearchResult.Id);
            Film film = JsonConvert.DeserializeObject<Film>(filmData);
            Backlog backlog = new Backlog
            {
                id = Guid.NewGuid(),
                Name = film.fullTitle,
                Type = "Film",
                ReleaseDate = film.releaseDate,
                ImageURL = film.image,
                TargetDate = date,
                Description = film.plot,
                Length = film.runtimeMins,
                Director = film.directors,
                Progress = 0,
                Units = "Minutes",
                ShowProgress = true,
                NotifTime = NotifTime == null ? TimeSpan.Zero : NotifTime,
                UserRating = -1,
                CreatedDate = DateTimeOffset.Now.Date.ToString("D", CultureInfo.InvariantCulture)
            };
            await CreateBacklogItemAsync(backlog);
        }

        /// <summary>
        /// Creates a music backlog
        /// </summary>
        /// <param name="title"></param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private async Task CreateMusicBacklogAsync()
        {
            try
            {
                string _date = DateInput != null ? DateInput.ToString("D", CultureInfo.InvariantCulture) : "None";
                string response = await RestClient.GetMusicResponse(NameInput);
                await Logger.Info($"Searching for album {NameInput}. Response: {response}");
                var musicData = JsonConvert.DeserializeObject<MusicData>(response);
                if(musicData != null)
                {
                    Music music = new Music
                    {
                        name = musicData.album.name,
                        artist = musicData.album.artist,
                        description = musicData.album.wiki == null ? "No summary found" : musicData.album.wiki.summary,
                        releaseDate = musicData.album.wiki == null ? "" : musicData.album.wiki.published,
                        image = musicData.album.image[2].Text
                    };
                    Backlog backlog = new Backlog
                    {
                        id = Guid.NewGuid(),
                        Name = music.name,
                        Type = "Album",
                        ReleaseDate = music.releaseDate,
                        ImageURL = music.image,
                        TargetDate = _date,
                        Description = music.description,
                        Director = music.artist,
                        Progress = 0,
                        Units = "Minutes",
                        ShowProgress = false,
                        NotifTime = NotifTime,
                        UserRating = -1,
                        CreatedDate = DateTimeOffset.Now.Date.ToString("D", CultureInfo.InvariantCulture)
                    };
                    await CreateBacklogItemAsync(backlog);
                    await Logger.Info("Succesfully created backlog");
                }
                else
                {
                    await ShowNotFoundDialogAsync();
                }
            }
            catch (Exception e)
            {
                await ShowErrorDialogAsync();
                await Logger.Error("Failed to create backlog", e);
            }
        }

        /// <summary>
        /// Searches for a book
        /// </summary>
        /// <param name="title"></param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private async Task SearchBookBacklogAsync()
        {
            try
            {
                string response = await RestClient.GetBookResponse(NameInput);
                await Logger.Info($"Trying to find book {NameInput}. Response {response}");
                var bookData = JsonConvert.DeserializeObject<BookInfo>(response);
                if(bookData.items.Count > 0)
                {
                    SearchResults.Clear();
                    foreach (var item in bookData.items)
                    {
                        try
                        {
                            SearchResults.Add(new Models.SearchResult
                            {
                                Id = item.id,
                                Name = item.volumeInfo.title,
                                Description = item.volumeInfo.publishedDate,
                                ImageURL = item.volumeInfo.imageLinks.thumbnail
                            });
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    _ = await ResultsDialog.ShowAsync();
                    await Logger.Info("Succesfully created backlog");
                }
                else
                {
                    await ShowNotFoundDialogAsync();
                }
            }
            catch (Exception e)
            {
                await Logger.Error("Failed to create backlog", e);
            }
        }

        private async Task CreateBookBacklogAsync(string date)
        {
            var title = NameInput;
            string response = await RestClient.GetBookResponse(title);
            await Logger.Info($"Trying to find book {title}. Response {response}");
            var bookData = JsonConvert.DeserializeObject<BookInfo>(response);
            Item item = new Item();
            foreach (var i in bookData.items)
            {
                if (i.id == SelectedSearchResult.Id)
                {
                    item = i;
                }
            }
            Book book = new Book
            {
                name = item.volumeInfo.title,
                author = string.Concat(item.volumeInfo.authors),
                desciption = item.volumeInfo.description,
                releaseDate = item.volumeInfo.publishedDate,
                image = item.volumeInfo.imageLinks.thumbnail,
                length = item.volumeInfo.pageCount
            };
            Backlog backlog = new Backlog
            {
                id = Guid.NewGuid(),
                Name = book.name,
                Type = "Book",
                ReleaseDate = book.releaseDate,
                ImageURL = book.image,
                TargetDate = date,
                Description = book.desciption,
                Director = book.author,
                Length = book.length,
                Progress = 0,
                Units = "Pages",
                ShowProgress = true,
                NotifTime = NotifTime,
                UserRating = -1,
                CreatedDate = DateTimeOffset.Now.Date.ToString("D", CultureInfo.InvariantCulture)
            };
            await CreateBacklogItemAsync(backlog);
        }

        /// <summary>
        /// Search for TV series and show results
        /// </summary>
        /// <param name="title"></param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private async Task SearchSeriesBacklogAsync()
        {
            try
            {
                string response = await RestClient.GetSeriesResponse(NameInput);
                await Logger.Info($"Trying to find series {NameInput}. Response: {response}");
                SeriesResult seriesResult = JsonConvert.DeserializeObject<SeriesResult>(response);
                if(seriesResult.results.Length > 0)
                {
                    SearchResults.Clear();
                    foreach (var result in seriesResult.results)
                    {
                        try
                        {
                            SearchResults.Add(new Models.SearchResult
                            {
                                Id = result.id,
                                Name = result.title,
                                Description = result.description,
                                ImageURL = result.image,
                            });
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    _ = await ResultsDialog.ShowAsync();
                    // SeriesResponse seriesResponse = seriesResult.results[0
                    await Logger.Info("Succesfully created backlog");
                }
                else
                {
                    await ShowNotFoundDialogAsync();
                }
            }
            catch (Exception e)
            {
                await Logger.Error("Failed to create backlog", e);
            }
        }

        private async Task CreateSeriesBacklogAsync(string date)
        {
            string seriesData = await RestClient.GetSeriesDataResponse(SelectedSearchResult.Id);
            Series series = JsonConvert.DeserializeObject<Series>(seriesData);
            Backlog backlog = new Backlog
            {
                id = Guid.NewGuid(),
                Name = series.fullTitle,
                Type = "TV",
                ReleaseDate = series.releaseDate,
                ImageURL = series.image,
                TargetDate = date,
                Description = series.plot,
                Length = series.TvSeriesInfo.Seasons.Count,
                Director = series.TvSeriesInfo.Creators,
                Progress = 0,
                Units = "Season(s)",
                ShowProgress = true,
                NotifTime = NotifTime,
                UserRating = -1,
                CreatedDate = DateTimeOffset.Now.Date.ToString("D", CultureInfo.InvariantCulture)
            };
            await CreateBacklogItemAsync(backlog);
        }

        /// <summary>
        /// Search for a game and show results
        /// </summary>
        /// <param name="title"></param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private async Task SearchGameBacklogAsync()
        {
            try
            {
                string response = await RestClient.GetGameResponse(NameInput);
                await Logger.Info($"Trying to find game {NameInput}. Response: {response}");
                var result = JsonConvert.DeserializeObject<GameResponse[]>(response);
                if(result.Length > 0)
                {
                    SearchResults.Clear();
                    foreach (var res in result)
                    {
                        string id = res.id.ToString();
                        string gameResponse = await RestClient.GetGameResult(id);
                        var gameResult = JsonConvert.DeserializeObject<GameResult[]>(gameResponse);
                        var gameCoverResponse = await RestClient.GetGameCover(gameResult[0].cover.ToString());
                        var gameCover = JsonConvert.DeserializeObject<GameCover[]>(gameCoverResponse);
                        try
                        {
                            string releaseDateResponse = await RestClient.GetGameReleaseResponse(gameResult[0].release_dates[0].ToString());
                            var releaseDateTimestamp = JsonConvert.DeserializeObject<GameReleaseDate[]>(releaseDateResponse);
                            var releaseDate = DateTimeOffset.FromUnixTimeSeconds(releaseDateTimestamp[0].date);
                            Game game = new Game
                            {
                                name = gameResult[0].name + $" ({releaseDate.Year})",
                                releaseDate = releaseDate.ToString("D"),
                                image = "https:" + gameCover[0].url
                            };
                            SearchResults.Add(new Models.SearchResult
                            {
                                Id = res.id.ToString(),
                                Name = game.name,
                                Description = game.releaseDate,
                                ImageURL = game.image
                            });
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    _ = await ResultsDialog.ShowAsync();
                    await Logger.Info("Succesfully created backlog");
                }
                else
                {
                    await ShowNotFoundDialogAsync();
                }
            }
            catch (Exception e)
            {
                await Logger.Error("Failed to create backlog", e);
            }
        }

        private async Task CreateGameBacklogAsync(string date)
        {
            string id = SelectedSearchResult.Id;
            string gameResponse = await RestClient.GetGameResult(id);
            var gameResult = JsonConvert.DeserializeObject<GameResult[]>(gameResponse);
            int companyID = await RestClient.GetCompanyID(gameResult[0].involved_companies[0].ToString());
            var gameCompanyResponse = await RestClient.GetGameCompanyResponse(companyID.ToString());
            var gameCompany = JsonConvert.DeserializeObject<GameCompany[]>(gameCompanyResponse);
            var gameCoverResponse = await RestClient.GetGameCover(gameResult[0].cover.ToString());
            var gameCover = JsonConvert.DeserializeObject<GameCover[]>(gameCoverResponse);
            string releaseDateResponse = await RestClient.GetGameReleaseResponse(gameResult[0].release_dates[0].ToString());
            var releaseDateTimestamp = JsonConvert.DeserializeObject<GameReleaseDate[]>(releaseDateResponse);
            var releaseDate = DateTimeOffset.FromUnixTimeSeconds(releaseDateTimestamp[0].date);
            Game game = new Game
            {
                name = gameResult[0].name + $" ({releaseDate.Year})",
                releaseDate = releaseDate.ToString("D"),
                company = gameCompany[0].name,
                image = "https:" + gameCover[0].url,
                storyline = gameResult[0].storyline
            };
            Backlog backlog = new Backlog
            {
                id = Guid.NewGuid(),
                Name = game.name,
                Type = "Game",
                ReleaseDate = game.releaseDate,
                ImageURL = game.image,
                TargetDate = date,
                Description = game.storyline,
                Length = 0,
                Director = game.company,
                Progress = 0,
                ShowProgress = false,
                NotifTime = NotifTime,
                UserRating = -1,
                CreatedDate = DateTimeOffset.Now.Date.ToString("D" ,CultureInfo.InvariantCulture)
            };
            await CreateBacklogItemAsync(backlog);
        }

        private async Task ShowNotFoundDialogAsync()
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = $"Couldn't find any results for {NameInput}",
                Content = $"Check if you've picked the right type or try entering the full title if you haven't done so. If that doesn't work, please go to \'Settings + more\' and send me the logs",
                CloseButtonText = "Ok"
            };
            _ = await contentDialog.ShowAsync();
        }

        private async Task ShowErrorDialogAsync()
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = $"Couldn't find any results for {NameInput}",
                Content = $"Could not create Backlog. Please go to \'Settings + more\' and send me the logs",
                CloseButtonText = "Ok"
            };
            _ = await contentDialog.ShowAsync();
        }

        private void CancelCreateAndGoBack()
        {
            PageStackEntry prevPage = Frame.BackStack.Last();
            try
            {
                Frame.Navigate(prevPage?.SourcePageType, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom});
            }
            catch
            {
                Frame.Navigate(prevPage?.SourcePageType);
            }
        }

        private void CancelCreation()
        {
            CancelCreateFunc();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private async Task SearchResultSelectedAsync()
        {
            ProgBar.Visibility = Visibility.Collapsed;
            if (SelectedSearchResult != null)
            {
                string date = DateInput != null ? DateInput.ToString("D", CultureInfo.InvariantCulture) : "None";
                ResultsDialog.Hide();
                switch (SelectedType)
                {
                    case "Film":
                        await CreateFilmBacklogAsync(date);
                        break;
                    case "TV":
                        await CreateSeriesBacklogAsync(date);
                        break;
                    case "Game":
                        await CreateGameBacklogAsync(date);
                        break;
                    case "Book":
                        await CreateBookBacklogAsync(date);
                        break;
                }
            }
        }

        private async void NameInput_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                await TrySearchBacklogAsync();
            }
        }
    }
}
