﻿using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using Microsoft.Graph;
using Newtonsoft.Json;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreatePage : Page
    {
        ObservableCollection<Backlog> backlogs { get; set; }
        bool checkboxChecked;
        string signedIn;
        bool isNetworkAvailable = false;
        GraphServiceClient graphServiceClient;
        public CreatePage()
        {
            this.InitializeComponent();
            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await Logger.WriteLogAsync("Navigated to Create page");
            signedIn = ApplicationData.Current.LocalSettings.Values["SignedIn"]?.ToString();
            if(isNetworkAvailable && signedIn == "Yes")
            {
                await SaveData.GetInstance().ReadDataAsync(true);
                backlogs = SaveData.GetInstance().GetBacklogs();
            }
            base.OnNavigatedTo(e);
        }

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

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            checkboxChecked = false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            checkboxChecked = true;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameInput.Text == "" || TypeComoBox.SelectedIndex < 0 || DatePicker.Date == null || TimePicker.Time == null)
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
                // Create backlog
            }    
        }

        private async Task CreateBacklog(string title, string date)
        {
            ProgBar.Visibility = Visibility.Visible;
            Backlog backlog = null;
            string type = TypeComoBox.SelectedItem.ToString();
            switch (type)
            {
                case "Film":
                    backlog = await CreateFilmBacklog(title, date, TimePicker.Time);
                    break;
                case "TV":
                    backlog = await CreateSeriesBacklog(title, date, TimePicker.Time);
                    break;
                case "Game":
                    backlog = await CreateGameBacklog(NameInput.Text, date, TimePicker.Time);
                    break;
                case "Book":
                    backlog = await CreateBookBacklog(NameInput.Text, date, TimePicker.Time);
                    break;
                case "Music":
                    backlog = await CreateMusicBacklog(NameInput.Text, date, TimePicker.Time);
                    break;
            }
            if(backlog != null)
            {
                backlogs.Add(backlog);
                SaveData.GetInstance().SaveSettings(backlogs);
                await SaveData.GetInstance().WriteDataAsync(signedIn == "Yes");
                Frame.Navigate(typeof(MainPage));
            }
            else
            {
                await ShowErrorDialog(title, type);
            }
            ProgBar.Visibility = Visibility.Collapsed;
        }

        private async Task<Backlog> CreateFilmBacklog(string title, string date, TimeSpan time)
        {
            try
            {
                string response = await RestClient.GetFilmResponse(title);
                await Logger.WriteLogAsync("Response: " + response);
                FilmResult filmResult = JsonConvert.DeserializeObject<FilmResult>(response);
                FilmResponse filmResponse = filmResult.results[0];
                string filmData = await RestClient.GetFilmDataResponse(filmResponse.id);
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
                return backlog;
            }
            catch (Exception e)
            {
                await Logger.WriteLogAsync(e.ToString());
                return null;
            }
        }


        private async Task<Backlog> CreateMusicBacklog(string title, string date, TimeSpan time)
        {
            try
            {
                string response = await RestClient.GetMusicResponse(title);
                await Logger.WriteLogAsync("Response: " + response);
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
                return backlog;
            }
            catch (Exception e)
            {
                await Logger.WriteLogAsync(e.ToString());
                return null;
            }
        }

        private async Task<Backlog> CreateBookBacklog(string title, string date, TimeSpan time)
        {
            try
            {
                string response = await RestClient.GetBookResponse(title);
                await Logger.WriteLogAsync("Response: " + response);
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
                    RemindEveryday = checkboxChecked
                };
                return backlog;
            }
            catch (Exception e)
            {
                await Logger.WriteLogAsync(e.ToString());
                return null;
            }
        }

        private async Task<Backlog> CreateSeriesBacklog(string title, string date, TimeSpan time)
        {
            try
            {
                string response = await RestClient.GetSeriesResponse(title);
                await Logger.WriteLogAsync("Response: " + response);
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
                    Units = "Season",
                    ShowProgress = true,
                    NotifTime = time,
                    RemindEveryday = checkboxChecked
                };
                return backlog;
            }
            catch (Exception e)
            {
                await Logger.WriteLogAsync(e.ToString());
                return null;
            }
        }

        private async Task<Backlog> CreateGameBacklog(string title, string date, TimeSpan time)
        {
            try
            {
                string response = await RestClient.GetGameResponse(title);
                await Logger.WriteLogAsync("Response: " + response);
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
                    RemindEveryday = checkboxChecked
                };
                return backlog;
                
            }
            catch (Exception e)
            {
                await Logger.WriteLogAsync(e.ToString());
                return null;
            }
        }

        private async Task ShowErrorDialog(string name, string type)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = $"Couldn't find {name}",
                Content = $"We couldn't find {name} of type {type}. Check if you've picked the right type or try entering the full title if you haven't done so.",
                CloseButtonText = "Ok"
            };
            ContentDialogResult result = await contentDialog.ShowAsync();
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}