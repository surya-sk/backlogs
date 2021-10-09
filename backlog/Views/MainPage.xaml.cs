using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.UI.Core;
using System.Net.NetworkInformation;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.ApplicationModel.Background;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Backlog> backlogs { get; set; }
        private ObservableCollection<Backlog> filmBacklogs { get; set; }
        private ObservableCollection<Backlog> tvBacklogs { get; set; }
        private ObservableCollection<Backlog> gameBacklogs { get; set; }
        private ObservableCollection<Backlog> musicBacklogs { get; set; }
        private ObservableCollection<Backlog> bookBacklogs { get; set; }
        GraphServiceClient graphServiceClient;

        readonly string toastTaskName = "ToastTask";

        bool checkboxChecked = false;
        bool isNetworkAvailable = false;
        string signedIn;
        public MainPage()
        {
            this.InitializeComponent();
            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            InitBacklogs();
        }

        private void InitBacklogs()
        {
            var readBacklogs = SaveData.GetInstance().GetBacklogs();
            backlogs = new ObservableCollection<Backlog>(readBacklogs.OrderByDescending(b => b.TargetDate));
            foreach(Backlog b in backlogs)
            {
                GenerateLiveTiles(b);
            }
            filmBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            tvBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            gameBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            musicBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Music.ToString()));
            bookBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Book.ToString()));
            ShowEmptyMessage();
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            ProgBar.Visibility = Visibility.Visible;
            signedIn = ApplicationData.Current.LocalSettings.Values["SignedIn"]?.ToString();
            if (isNetworkAvailable && signedIn == "Yes")
            {
                graphServiceClient = await SaveData.GetInstance().GetGraphServiceClient();
                SigninButton.Visibility = Visibility.Collapsed;
                ProfileButton.Visibility = Visibility.Visible;
                await SaveData.GetInstance().ReadDataAsync(true);
                PopulateBacklogs();
            }
            await BuildNotifactionQueue();
            ProgBar.Visibility = Visibility.Collapsed;
            base.OnNavigatedTo(e);
        }

        private void PopulateBacklogs()
        {
            ObservableCollection<Backlog> readBacklogs = SaveData.GetInstance().GetBacklogs();
            var _backlogs = new ObservableCollection<Backlog>(readBacklogs.OrderByDescending(b => b.TargetDate)); // sort by last created
            var _filmBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            var _tvBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            var _gameBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            var _musicBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Music.ToString()));
            var _bookBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Book.ToString()));
            backlogs.Clear();
            filmBacklogs.Clear();
            tvBacklogs.Clear();
            gameBacklogs.Clear();
            musicBacklogs.Clear();
            bookBacklogs.Clear();
            EmtpyListText.Visibility = Visibility.Collapsed;
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
        }

        private void ShowEmptyMessage()
        {
            ObservableCollection<Backlog>[] _backlogs = { backlogs, filmBacklogs, tvBacklogs, gameBacklogs, musicBacklogs, bookBacklogs };
            TextBlock[] textBlocks = { EmtpyListText, EmtpyFilmsText, EmtpyTVText, EmtpyGamesText, EmtpyMusicText, EmtpyBooksText };
            for (int i = 0; i < _backlogs.Length; i++)
            {
                if(_backlogs[i].Count <=0)
                {
                    textBlocks[i].Visibility = Visibility.Visible;
                    if(i > 0)
                    {
                        textBlocks[i].Text = $"Nothing to see here. Add some!";
                    }
                }
            }
        }

        private void BacklogView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id);
        }

        private async void SigninButton_Click(object sender, RoutedEventArgs e)
        {
            signedIn = ApplicationData.Current.LocalSettings.Values["SignedIn"]?.ToString();
            if (isNetworkAvailable)
            {
                if (signedIn != "Yes")
                {
                    graphServiceClient = await SaveData.GetInstance().GetGraphServiceClient();
                    ApplicationData.Current.LocalSettings.Values["SignedIn"] = "Yes";
                    Frame.Navigate(typeof(MainPage));
                }
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "No Internet",
                    Content = "You need to be connected to sign-in",
                    CloseButtonText = "Ok"
                };
                ContentDialogResult result = await contentDialog.ShowAsync();
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await CreateBacklogDialog.ShowAsync();
            if(result == ContentDialogResult.Secondary)
            {
                NameInput.Text = string.Empty;
                TypeComoBox.SelectedIndex = -1;
                ErrorText.Visibility = Visibility.Collapsed;
            }
        }

        private void TypeComoBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = TypeComoBox.SelectedValue.ToString();
            switch(value)
            {
                case "Film":
                    NameInput.PlaceholderText = "Inception 2010 (optional)";
                    break;
                case "TV":
                    NameInput.PlaceholderText = "Hannibal";
                    break;
                case "Music":
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

        private async void CreateBacklogDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (NameInput.Text == "" || TypeComoBox.SelectedIndex < 0 || DatePicker.Date == null || TimePicker.Time == null) 
            {
                ErrorText.Text = "Fill out all the fields";
                ErrorText.Visibility = Visibility.Visible;
                args.Cancel = true;
            }
            else
            {
               
                string title = NameInput.Text.Replace(" ", string.Empty);
                string date = DatePicker.Date.ToString("d");
                DateTimeOffset dateTime = DateTimeOffset.Parse(date).Add(TimePicker.Time);
                int result = DateTimeOffset.Compare(dateTime, DateTimeOffset.Now);
                if(result < 0)
                {
                    ErrorText.Visibility = Visibility.Visible;
                    ErrorText.Text = "The date and time you've chosen are in the past!";
                    args.Cancel = true;
                }
                else
                { 
                    EmtpyListText.Visibility = Visibility.Collapsed;
                    await CreateBacklog(title, date);
                }
            }
        }

        private async Task CreateBacklog(string title, string date)
        {
            CreationProgBar.Visibility = Visibility.Visible;
            switch (TypeComoBox.SelectedItem.ToString())
            {
                case "Film":
                    await CreateFilmBacklog(title, date, TimePicker.Time);
                    EmtpyFilmsText.Visibility = Visibility.Collapsed;
                    break;
                case "TV":
                    await CreateSeriesBacklog(title, date, TimePicker.Time);
                    EmtpyTVText.Visibility = Visibility.Collapsed;
                    break;
                case "Game":
                    await CreateGameBacklog(NameInput.Text, date, TimePicker.Time);
                    EmtpyGamesText.Visibility = Visibility.Collapsed;
                    break;
                case "Book":
                    await CreateBookBacklog(NameInput.Text, date, TimePicker.Time);
                    EmtpyBooksText.Visibility = Visibility.Collapsed;
                    break;
                case "Music":
                    await CreateMusicBacklog(NameInput.Text, date, TimePicker.Time);
                    EmtpyMusicText.Visibility = Visibility.Collapsed;
                    break;
            }
            checkboxChecked = false;
            SaveData.GetInstance().SaveSettings(backlogs);
            await SaveData.GetInstance().WriteDataAsync(signedIn == "Yes");
            await BuildNotifactionQueue();
            CreationProgBar.Visibility = Visibility.Collapsed;
        }

        private async Task CreateFilmBacklog(string title, string date, TimeSpan time)
        {
            string response = await RestClient.GetFilmResponse(title);
            FilmResult filmResult = JsonConvert.DeserializeObject<FilmResult>(response);
            FilmResponse filmResponse = filmResult.results[0];
            string filmData = await RestClient.GetFilmDataResponse(filmResponse.id);
            if (filmData != null)
            {
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
                    NotifTime = time,
                    RemindEveryday = checkboxChecked
                };
                backlogs.Add(backlog);
                filmBacklogs.Add(backlog);
            }
        }

        private async Task CreateMusicBacklog(string title, string date, TimeSpan time)
        {
            string response = await RestClient.GetMusicResponse(title);
            var musicData = JsonConvert.DeserializeObject<MusicData>(response);
            Music music = new Music
            {
                name = musicData.album.name,
                artist = musicData.album.artist,
                description = musicData.album.wiki == null ? "" : musicData.album.wiki.summary,
                releaseDate = musicData.album.wiki == null ? "" : musicData.album.wiki.published,
                image = musicData.album.image[2].Text
            };
            Backlog backlog = new Backlog
            {
                id = Guid.NewGuid(),
                Name = music.name,
                Type = "Music",
                ReleaseDate = music.releaseDate,
                ImageURL = music.image,
                TargetDate = date,
                Description = music.description,
                Director = music.artist,
                Progress = 0,
                Units = "Minutes",
                ShowProgress = false,
                NotifTime = time,
                RemindEveryday = checkboxChecked
            };
            backlogs.Add(backlog);
            musicBacklogs.Add(backlog);
        }

        private async Task CreateBookBacklog(string title, string date, TimeSpan time)
        {
            string response = await RestClient.GetBookResponse(title);
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
            if(book != null)
            {
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
                    RemindEveryday = checkboxChecked
                };
                backlogs.Add(backlog);
                bookBacklogs.Add(backlog);
            }
        }

        private async Task CreateSeriesBacklog(string titile, string date, TimeSpan time)
        {
            string response = await RestClient.GetSeriesResponse(titile);
            SeriesResult seriesResult = JsonConvert.DeserializeObject<SeriesResult>(response);
            SeriesResponse seriesResponse = seriesResult.results[0];
            string seriesData = await RestClient.GetSeriesDataResponse(seriesResponse.id);
            if(seriesData != null)
            {
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
                    Units = "Season",
                    ShowProgress = true,
                    NotifTime = time,
                    RemindEveryday = checkboxChecked
                };
                backlogs.Add(backlog);
                tvBacklogs.Add(backlog);
            }
        }

        private async Task CreateGameBacklog(string title, string date, TimeSpan time)
        {
            string response = await RestClient.GetGameResponse(title);
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
            if (game != null)
            {
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
                    RemindEveryday = checkboxChecked
                };
                backlogs.Add(backlog);
                gameBacklogs.Add(backlog);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private bool IsBackgroundTaskRegistered(string taskName)
        {
            if (BackgroundTaskHelper.IsBackgroundTaskRegistered(taskName))
            {
                // Background task already registered.
                return true;
            }

            return false;
        }

        private void RateButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private async Task BuildNotifactionQueue()
        {
            foreach (Backlog b in backlogs)
            {
                ApplicationDataContainer notifSettings = ApplicationData.Current.LocalSettings;
                int? notifSent = (int?)notifSettings.Values[b.id.ToString()];
                if (notifSent != 1)
                {
                    var builder = new ToastContentBuilder()
                    .AddText($"Hey there!", hintMaxLines: 1)
                    .AddText($"You wanted to check out {b.Name} by {b.Director} today. Here's your reminder!", hintMaxLines: 2)
                    .AddHeroImage(new Uri(b.ImageURL));
                    DateTimeOffset date = DateTimeOffset.Parse(b.TargetDate).Add(b.NotifTime);
                    ScheduledToastNotification toastNotification = new ScheduledToastNotification(builder.GetXml(), date);
                    ToastNotificationManager.CreateToastNotifier().AddToSchedule(toastNotification);
                    if (b.RemindEveryday)
                    {
                        if(IsBackgroundTaskRegistered(toastTaskName))
                        {
                            return;
                        }

                        await BackgroundExecutionManager.RequestAccessAsync();
                        BackgroundTaskHelper.Register(toastTaskName, new TimeTrigger(1440, false));
                    }
                    notifSettings.Values[b.id.ToString()] = 1;
                }
            }
        }

        private void GenerateLiveTiles(Backlog b)
        {
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {

                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        DisplayName = "Backlogs",
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = b.ImageURL,
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = b.Name,
                                    HintWrap = true,
                                    HintMaxLines = 2
                                },
                                new AdaptiveText()
                                {
                                    Text = b.Type,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },
                                new AdaptiveText()
                                {
                                    Text = b.TargetDate,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = "Backlogs",
                        Content = new TileBindingContentAdaptive()
                        {
                            PeekImage = new TilePeekImage()
                            {
                                Source = b.ImageURL
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = b.Name
                                },
                                new AdaptiveText()
                                {
                                    Text = $"{b.Type} - {b.Director}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = $"You have this set for {b.TargetDate}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                        }
                    },
                }
            };

            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            checkboxChecked = true;
        }

        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            checkboxChecked = false;
        }
    }
}
