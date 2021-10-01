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
        StorageFolder roamingFolder = ApplicationData.Current.RoamingFolder;
        string fileName = "backlogs.txt";
        public MainPage()
        {
            this.InitializeComponent();
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            InitBacklogs();
        }

        private void InitBacklogs()
        {
            backlogs = SaveData.GetInstance().GetBacklogs();
            filmBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            tvBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            gameBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            musicBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Music.ToString()));
            bookBacklogs = new ObservableCollection<Backlog>(backlogs.Where(b => b.Type == BacklogType.Book.ToString()));
            ShowEmptyMessage();
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
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
            Debug.WriteLine(selectedBacklog.id);
            Frame.Navigate(typeof(BacklogPage), selectedBacklog.id);
        }

        private void SigninButton_Click(object sender, RoutedEventArgs e)
        {

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
            if (NameInput.Text == "" || TypeComoBox.SelectedIndex < 0 || DatePicker.Date == null) 
            {
                ErrorText.Text = "Fill out all the fields";
                ErrorText.Visibility = Visibility.Visible;
                args.Cancel = true;
            }
            else
            {
                CreationProgBar.Visibility = Visibility.Visible;
                string title = NameInput.Text.Replace(" ", string.Empty);
                string date = DatePicker.Date.ToString("D");
                EmtpyListText.Visibility = Visibility.Collapsed;
                switch(TypeComoBox.SelectedItem.ToString())
                {
                    case "Film":
                        await CreateFilmBacklog(title, date);
                        EmtpyFilmsText.Visibility = Visibility.Collapsed;
                        break;
                    case "TV":
                        await CreateSeriesBacklog(title, date);
                        EmtpyTVText.Visibility = Visibility.Collapsed;
                        break;
                    case "Game":
                        await CreateGameBacklog(NameInput.Text, date);
                        EmtpyGamesText.Visibility = Visibility.Collapsed;
                        break;
                    case "Book":
                        await CreateBookBacklog(NameInput.Text, date);
                        EmtpyBooksText.Visibility = Visibility.Collapsed;
                        break;
                    case "Music":
                        await CreateMusicBacklog(NameInput.Text, date);
                        EmtpyMusicText.Visibility = Visibility.Collapsed;
                        break;
                }
                SaveData.GetInstance().SaveSettings(backlogs);
                await SaveData.GetInstance().WriteDataAsync();
                CreationProgBar.Visibility = Visibility.Collapsed;
            }
        }

        private async Task CreateFilmBacklog(string title, string date)
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
                    Progress = 0
                };
                backlogs.Add(backlog);
                filmBacklogs.Add(backlog);
            }
        }

        private async Task CreateMusicBacklog(string title, string date)
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
                Progress = 0
            };
            backlogs.Add(backlog);
            musicBacklogs.Add(backlog);
        }

        private async Task CreateBookBacklog(string title, string date)
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
                    Progress = 0
                };
                backlogs.Add(backlog);
                bookBacklogs.Add(backlog);
            }
        }

        private async Task CreateSeriesBacklog(string titile, string date)
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
                    Length = series.runtimeMin,
                    Director = series.directors,
                    Progress = 0
                };
                backlogs.Add(backlog);
                tvBacklogs.Add(backlog);
            }
        }

        private async Task CreateGameBacklog(string title, string date)
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
                name = gameResult[0].name + " " + releaseDate.Year,
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
                     Progress = 0
                };
                backlogs.Add(backlog);
                gameBacklogs.Add(backlog);
            }
        }
    }
}
