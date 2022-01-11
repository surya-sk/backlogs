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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreatePage : Page
    {
        ObservableCollection<Backlog> backlogs { get; set; }
        bool signedIn;
        bool isNetworkAvailable = false;
        GraphServiceClient graphServiceClient;
        public CreatePage()
        {
            this.InitializeComponent();
            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            signedIn = Settings.IsSignedIn;
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
                    Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
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
        private void TypeComoBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = TypeComoBox.SelectedValue.ToString();
            switch (value)
            {
                case "Film":
                    NameInput.PlaceholderText = "Inception 2010";
                    break;
                case "TV":
                    NameInput.PlaceholderText = "Hannibal";
                    break;
                case "Album":
                    NameInput.PlaceholderText = "Radiohead - Amnesiac";
                    break;
                case "Game":
                    NameInput.PlaceholderText = "Assassins Creed 2";
                    break;
                case "Book":
                    NameInput.PlaceholderText = "Never Let Me Go";
                    break;
            }
        }

        /// <summary>
        /// Validate user input and proceed to create the backlog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
           try
            {
                await Logger.Info("Creating backlog.....");
                string title = NameInput.Text;

                if (title == "" || TypeComoBox.SelectedIndex < 0)
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
                        string date = DatePicker.SelectedDates[0].ToString("d", CultureInfo.InvariantCulture);
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
                    await CreateBacklog(title);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                await Logger.Error("Failed to create backlog", ex);
            }
        }

        /// <summary>
        /// Search for the backlog metadata and create it
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private async Task CreateBacklog(string title)
        {
            ProgBar.Visibility = Visibility.Visible;
            string date = DatePicker.SelectedDates.Count > 0 ? DatePicker.SelectedDates[0].ToString("d", CultureInfo.InvariantCulture) : "None";
            string type = TypeComoBox.SelectedItem.ToString();
            switch (type)
            {
                case "Film":
                    await SearchFilmBacklog(title, date, TimePicker.Time);
                    break;
                case "TV":
                    await CreateSeriesBacklog(title, date, TimePicker.Time);
                    break;
                case "Game":
                    await CreateGameBacklog(NameInput.Text, date, TimePicker.Time);
                    break;
                case "Book":
                    await CreateBookBacklog(NameInput.Text, date, TimePicker.Time);
                    break;
                case "Album":
                    await CreateMusicBacklog(NameInput.Text, date, TimePicker.Time);
                    break;
            }
            ProgBar.Visibility = Visibility.Collapsed;
        }

        private async Task CreateBacklog(Backlog backlog)
        {
            if (backlog != null)
            {
                backlogs.Add(backlog);
                SaveData.GetInstance().SaveSettings(backlogs);
                await SaveData.GetInstance().WriteDataAsync(signedIn);
                try
                {
                    Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                }
                catch
                {
                    Frame.Navigate(typeof(MainPage));
                }
            }
            else
            {
                await ShowErrorDialog(NameInput.Text, TypeComoBox.SelectedItem.ToString());
            }
        }

        private async Task SearchFilmBacklog(string title, string date, TimeSpan time)
        {
            try
            {
                string response = await RestClient.GetFilmResponse(title);
                await Logger.Info($"Trying to find film {title}. Response: {response}");
                FilmResult filmResult = JsonConvert.DeserializeObject<FilmResult>(response);
                ObservableCollection<Models.SearchResult> results = new ObservableCollection<Models.SearchResult>();
                foreach(var result in filmResult.results)
                {
                    results.Add(new Models.SearchResult
                    {
                        Id = result.id,
                        Name = result.title,
                        Description = result.description,
                        ImageURL = result.image
                    });
                }
                ResultsListView.ItemsSource = results;
                _ = await ResultsDialog.ShowAsync();
                //FilmResponse filmResponse = filmResult.results[0
                await Logger.Info("Succesfully created backlog");
            }
            catch (Exception e)
            {
                await Logger.Error("Failed to find film.", e);
            }
        }
        private async Task CreateMusicBacklog(string title, string date, TimeSpan time)
        {
            try
            {
                string response = await RestClient.GetMusicResponse(title);
                await Logger.Info($"Searching for album {title}. Response: {response}");
                var musicData = JsonConvert.DeserializeObject<MusicData>(response);
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
                    TargetDate = date,
                    Description = music.description,
                    Director = music.artist,
                    Progress = 0,
                    Units = "Minutes",
                    ShowProgress = false,
                    NotifTime = time,
                    UserRating = -1
                };
                await CreateBacklog(backlog);
                await Logger.Info("Succesfully created backlog");
            }
            catch (Exception e)
            {
                await Logger.Error("Failed to create backlog", e);
            }
        }

        private async Task CreateBookBacklog(string title, string date, TimeSpan time)
        {
            try
            {
                string response = await RestClient.GetBookResponse(title);
                await Logger.Info($"Trying to find book {title}. Response {response}");
                var bookData = JsonConvert.DeserializeObject<BookInfo>(response);
                Book book = new Book
                {
                    name = bookData.items[0].volumeInfo.title,
                    author = string.Concat(bookData.items[0].volumeInfo.authors),
                    desciption = bookData.items[0].volumeInfo.description,
                    releaseDate = bookData.items[0].volumeInfo.publishedDate,
                    image = bookData.items[0].volumeInfo.imageLinks.thumbnail,
                    length = bookData.items[0].volumeInfo.pageCount
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
                    NotifTime = time,
                    UserRating = -1
                };
                await CreateBacklog(backlog);
                await Logger.Info("Succesfully created backlog");
            }
            catch (Exception e)
            {
                await Logger.Error("Failed to create backlog", e);
            }
        }

        private async Task CreateSeriesBacklog(string title, string date, TimeSpan time)
        {
            try
            {
                string response = await RestClient.GetSeriesResponse(title);
                await Logger.Info($"Trying to find series {title}. Response: {response}");
                SeriesResult seriesResult = JsonConvert.DeserializeObject<SeriesResult>(response);
                SeriesResponse seriesResponse = seriesResult.results[0];
                string seriesData = await RestClient.GetSeriesDataResponse(seriesResponse.id);
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
                    NotifTime = time,
                    UserRating = -1
                };
                await CreateBacklog(backlog);
                await Logger.Info("Succesfully created backlog");
            }
            catch (Exception e)
            {
                await Logger.Error("Failed to create backlog", e);
            }
        }

        private async Task CreateGameBacklog(string title, string date, TimeSpan time)
        {
            try
            {
                string response = await RestClient.GetGameResponse(title);
                await Logger.Info($"Trying to find game {title}. Response: {response}");
                var result = JsonConvert.DeserializeObject<GameResponse[]>(response);
                string id = result[0].id.ToString();
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
                    NotifTime = time,
                    UserRating = -1
                };
                await CreateBacklog(backlog);
                await Logger.Info("Succesfully created backlog");
            }
            catch (Exception e)
            {
                await Logger.Error("Failed to create backlog", e);
            }
        }

        private async Task ShowErrorDialog(string name, string type)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = $"Couldn't find {name}",
                Content = $"Couldn't find {name}. Check if you've picked the right type or try entering the full title if you haven't done so. If that doesn't work, please go to \'Settings + more\' and send me the logs",
                CloseButtonText = "Ok"
            };
            _ = await contentDialog.ShowAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom});
            }
            catch
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private async void ResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProgBar.Visibility = Visibility.Collapsed;
            var selectedItem = ResultsListView.SelectedItem as Models.SearchResult;
            if(selectedItem != null)
            {
                string date = DatePicker.SelectedDates.Count > 0 ? DatePicker.SelectedDates[0].ToString("d", CultureInfo.InvariantCulture) : "None";
                var time = TimePicker.Time;
                ResultsDialog.Hide();
                switch (TypeComoBox.SelectedItem.ToString())
                {
                    case "Film":
                        await CreateFilmBacklog(selectedItem, date, time);
                        break;
                }
            }
        }

        private async Task CreateFilmBacklog(Models.SearchResult selectedItem, string date, TimeSpan time)
        {
            string filmData = await RestClient.GetFilmDataResponse(selectedItem.Id);
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
                NotifTime = time == null ? TimeSpan.Zero : time,
                UserRating = -1
            };
            await CreateBacklog(backlog);
        }
    }
}
