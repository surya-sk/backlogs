using Backlogs.Constants;
using Backlogs.Models;
using Backlogs.Services;
using Backlogs.Utils;
using MvvmHelpers.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;
using SearchResult = Backlogs.Models.SearchResult;

namespace Backlogs.ViewModels
{
    public class CreateBacklogViewModel: INotifyPropertyChanged
    {
        private string m_selectedType;
        private int m_selectedIndex = -1;
        private string m_placeholderText = "Enter name";
        private string m_nameInput;
        private DateTimeOffset m_dateInput = DateTimeOffset.MinValue;
        private TimeSpan m_notifTime;
        private bool m_enableNotificationToggle;
        private bool m_showNotificationToggle;
        private bool m_showNotificationOptions;
        private SearchResult m_selectedResult;
        private string m_searchResultTitle;
        private bool m_createButtonEnabled;
        private bool m_isBusy;
        private bool m_SignedIn;
        private readonly INavigation m_navigationService;
        private readonly IDialogHandler m_dialogHandler;
        private readonly IToastNotificationService m_toastNotificationService;
        private readonly IUserSettings m_settings;
        private readonly IFileHandler m_fileHandler;

        public ObservableCollection<Backlog> Backlogs;
        public ObservableCollection<SearchResult> SearchResults;

        public ICommand SearchBacklog { get; }
        public ICommand OpenSettings { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime Today = DateTime.Today;

        #region Properties
        public string SelectedType
        {
            get => m_selectedType;
            set
            {
                if (m_selectedType != value)
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
                if (m_notifTime != value)
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

        public SearchResult SelectedSearchResult
        {
            get => m_selectedResult;
            set
            {
                if (m_selectedResult != value)
                {
                    m_selectedResult = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSearchResult)));
                    CreateButtonEnabled = true;
                }
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

        public bool CreateButtonEnabled
        {
            get => m_createButtonEnabled;
            set
            {
                m_createButtonEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreateButtonEnabled)));
            }
        }

        public bool IsBusy
        {
            get => m_isBusy;
            set
            {
                m_isBusy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }
        #endregion

        public CreateBacklogViewModel(INavigation navigationService, IDialogHandler dialogHandler,
            IToastNotificationService toastNotificationService, IUserSettings userSettings, IFileHandler fileHander)
        {
            SearchResults = new ObservableCollection<SearchResult>();
            SearchBacklog = new AsyncCommand(TrySearchBacklogAsync);
            OpenSettings = new Command(NavigateToSettingsPage);

            m_navigationService = navigationService;
            m_dialogHandler = dialogHandler;
            m_toastNotificationService = toastNotificationService;
            m_settings = userSettings;
            m_fileHandler = fileHander;

            m_SignedIn = m_settings.Get<bool>(SettingsConstants.IsSignedIn);
        }

        /// <summary>
        /// Sync backlogs if connected to the internet
        /// </summary>
        /// <returns></returns>
        public async Task SyncBacklogs()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                await m_fileHandler.WriteLogsAsync("Fetching backlogs...");
                if (m_SignedIn)
                {
                    await BacklogsManager.GetInstance().ReadDataAsync(true);
                    Backlogs = BacklogsManager.GetInstance().GetBacklogs();
                }
                else
                {
                    await BacklogsManager.GetInstance().ReadDataAsync();
                    Backlogs = BacklogsManager.GetInstance().GetBacklogs();
                }
            }
            else
            {
               await m_fileHandler.WriteLogsAsync("Not connected to the internet");
                await m_dialogHandler.ShowErrorDialogAsync("No internet", "You need the internet to create backlogs", "Ok");
            }
        }

        /// <summary>
        /// Show hint text according to the selected type
        /// </summary>
        /// <param name="value"></param>
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
        public async Task TrySearchBacklogAsync()
        {
            try
            {
                await m_fileHandler.WriteLogsAsync("Creating backlog.....");
            }
            catch (Exception ex)
            {
                await m_fileHandler.WriteLogsAsync("Error", ex);
            }

            try
            {
                if (m_nameInput == "" || SelectedIndex < 0)
                {
                    await m_dialogHandler.ShowErrorDialogAsync("Missing fields", "Please fill in all the values", "Ok");
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
                                await m_dialogHandler.ShowErrorDialogAsync("Invalid date and time time", "Pleae pick a time", "Ok");
                                return;
                            }
                            DateTimeOffset dateTime = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture).Add(NotifTime);
                            int diff = DateTimeOffset.Compare(dateTime, DateTimeOffset.Now);
                            if (diff < 0)
                            {
                                await m_dialogHandler.ShowErrorDialogAsync("Invalid time", "The date and time you've chosen are in the past!", "Ok");
                                return;
                            }
                        }
                        else
                        {
                            DateTimeOffset dateTime = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture);
                            int diff = DateTime.Compare(DateTime.Today, DateInput.DateTime);
                            if (diff > 0)
                            {
                                await m_dialogHandler.ShowErrorDialogAsync("Invalid date and time", "The date and time you've chosen are in the past!", "Ok");
                                return;
                            }
                        }
                    }
                    SearchResultTitle = $"Showing results for \"{m_nameInput}\". Click the one you'd like to add";
                    await SearchBacklogAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                await m_fileHandler.WriteLogsAsync("Failed to create backlog", ex);
            }
        }

        /// <summary>
        /// Search for the backlog metadata
        /// </summary>
        /// <returns></returns>
        private async Task SearchBacklogAsync()
        {
            IsBusy = true;
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
            IsBusy = false;
        }

        /// <summary>
        /// Creates a backlog item
        /// </summary>
        /// <param name="backlog"></param>
        /// <returns></returns>
        private async Task CreateBacklogItemAsync(Backlog backlog)
        {
            IsBusy = true;
            if (backlog != null)
            {
                Backlogs.Add(backlog);
                BacklogsManager.GetInstance().SaveSettings(Backlogs);
                if (backlog.TargetDate != "None" && backlog.NotifTime != TimeSpan.Zero)
                {
                    m_toastNotificationService.CreateToastNotification(backlog);   
                }
                await BacklogsManager.GetInstance().WriteDataAsync(m_SignedIn);
                NavToPrevPage();
            }
            else
            {
                await ShowErrorDialogAsync();
                IsBusy = false;
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
                string response = await RestClient.GetFilmResponse(m_nameInput);
                await m_fileHandler.WriteLogsAsync($"Trying to find film {m_nameInput}. Response: {response}");
                FilmResult filmResult = JsonConvert.DeserializeObject<FilmResult>(response);
                if (filmResult.results.Length > 0)
                {
                    SearchResults.Clear();
                    foreach (var result in filmResult.results)
                    {
                        try
                        {
                            if (String.IsNullOrEmpty(result.image)) continue;
                            SearchResults.Add(new SearchResult
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
                    SelectedSearchResult = await m_dialogHandler.ShowSearchResultsDialogAsync(m_nameInput, SearchResults);
                    await SearchResultSelectedAsync();
                    await m_fileHandler.WriteLogsAsync("Succesfully created backlog");
                }
                else
                {
                    await ShowNotFoundDialogAsync();
                }
            }
            catch (Exception e)
            {
                await m_fileHandler.WriteLogsAsync("Failed to find film.", e);
            }
        }

        /// <summary>
        /// Create a film backlog
        /// </summary>
        /// <param name="date"></param>
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
        /// <returns></returns>
        private async Task CreateMusicBacklogAsync()
        {
            try
            {
                string _date = DateInput != null ? DateInput.ToString("D", CultureInfo.InvariantCulture) : "None";
                string response = await RestClient.GetMusicResponse(m_nameInput);
                await m_fileHandler.WriteLogsAsync($"Searching for album {m_nameInput}. Response: {response}");
                var musicData = JsonConvert.DeserializeObject<MusicData>(response);
                if (musicData != null)
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
                   await m_fileHandler.WriteLogsAsync("Succesfully created backlog");
                }
                else
                {
                    await ShowNotFoundDialogAsync();
                }
            }
            catch (Exception e)
            {
                await ShowErrorDialogAsync();
                await m_fileHandler.WriteLogsAsync("Failed to create backlog", e);
            }
        }

        /// <summary>
        /// Searches for a book
        /// </summary>
        /// <returns></returns>
        private async Task SearchBookBacklogAsync()
        {
            try
            {
                string response = await RestClient.GetBookResponse(m_nameInput);
                await m_fileHandler.WriteLogsAsync($"Trying to find book {m_nameInput}. Response {response}");
                var bookData = JsonConvert.DeserializeObject<BookInfo>(response);
                if (bookData.items.Count > 0)
                {
                    SearchResults.Clear();
                    foreach (var item in bookData.items)
                    {
                        try
                        {
                            if (String.IsNullOrEmpty(item.volumeInfo.imageLinks.thumbnail)) continue;
                            SearchResults.Add(new SearchResult
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
                    SelectedSearchResult = await m_dialogHandler.ShowSearchResultsDialogAsync(m_nameInput, SearchResults);
                    await SearchResultSelectedAsync();
                    await m_fileHandler.WriteLogsAsync("Succesfully created backlog");
                }
                else
                {
                    await ShowNotFoundDialogAsync();
                }
            }
            catch (Exception e)
            {
                await m_fileHandler.WriteLogsAsync("Failed to create backlog", e);
            }
        }

        /// <summary>
        /// Creates a book backlog
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private async Task CreateBookBacklogAsync(string date)
        {
            var title = m_nameInput;
            string response = await RestClient.GetBookResponse(title);
            await m_fileHandler.WriteLogsAsync($"Trying to find book {title}. Response {response}");
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
        /// <returns></returns>
        private async Task SearchSeriesBacklogAsync()
        {
            try
            {
                string response = await RestClient.GetSeriesResponse(m_nameInput);
                await m_fileHandler.WriteLogsAsync($"Trying to find series {m_nameInput}. Response: {response}");
                SeriesResult seriesResult = JsonConvert.DeserializeObject<SeriesResult>(response);
                if (seriesResult.results.Length > 0)
                {
                    SearchResults.Clear();
                    foreach (var result in seriesResult.results)
                    {
                        try
                        {
                            if (String.IsNullOrEmpty(result.image)) continue;
                            SearchResults.Add(new SearchResult
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
                    SelectedSearchResult = await m_dialogHandler.ShowSearchResultsDialogAsync(m_nameInput, SearchResults);
                    await SearchResultSelectedAsync();
                    await m_fileHandler.WriteLogsAsync("Succesfully created backlog");
                }
                else
                {
                    await ShowNotFoundDialogAsync();
                }
            }
            catch (Exception e)
            {
                await m_fileHandler.WriteLogsAsync("Failed to create backlog", e);
            }
        }

        /// <summary>
        /// Create a TV series backlog
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
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
        /// <returns></returns>
        private async Task SearchGameBacklogAsync()
        {
            try
            {
                string response = await RestClient.GetGameResponse(m_nameInput);
                await m_fileHandler.WriteLogsAsync($"Trying to find game {m_nameInput}. Response: {response}");
                var result = JsonConvert.DeserializeObject<GameResponse[]>(response);
                if (result.Length > 0)
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
                            if (String.IsNullOrEmpty(game.image)) continue;
                            SearchResults.Add(new SearchResult
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
                    SelectedSearchResult = await m_dialogHandler.ShowSearchResultsDialogAsync(m_nameInput, SearchResults);
                    await SearchResultSelectedAsync();
                    await m_fileHandler.WriteLogsAsync("Succesfully created backlog");
                }
                else
                {
                    await ShowNotFoundDialogAsync();
                }
            }
            catch (Exception e)
            {
                await m_fileHandler.WriteLogsAsync("Failed to create backlog", e);
            }
        }

        /// <summary>
        /// Create a game backlog
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
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
                CreatedDate = DateTimeOffset.Now.Date.ToString("D", CultureInfo.InvariantCulture)
            };
            await CreateBacklogItemAsync(backlog);
        }

        private async Task ShowNotFoundDialogAsync()
        {
            await m_dialogHandler.ShowErrorDialogAsync($"Couldn't find any results for {m_nameInput}", $"Check if you've picked the right type or try entering the full title if you haven't done so. If that doesn't work, please go to \'Settings + more\' and send me the logs", "Ok");
        }

        private async Task ShowErrorDialogAsync()
        {
            await m_dialogHandler.ShowErrorDialogAsync($"Couldn't find any results for {m_nameInput}", $"Could not create Backlog. Please go to \'Settings + more\' and send me the logs", "Ok");
        }

        private void NavToPrevPage()
        {
            m_navigationService.GoBack<CreateBacklogViewModel>();
        }

        public void GoBack()
        {
            NavToPrevPage();
        }

        /// <summary>
        /// Create a backlog based on selected search result
        /// </summary>
        /// <returns></returns>
        private async Task SearchResultSelectedAsync()
        {
            IsBusy = true;
            if (SelectedSearchResult != null)
            {
                string date = DateInput != DateTimeOffset.MinValue ? DateInput.ToString("D", CultureInfo.InvariantCulture) : "None";
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

        private void NavigateToSettingsPage()
        {
            m_navigationService.NavigateTo<SettingsViewModel>();
        }
    }
}
