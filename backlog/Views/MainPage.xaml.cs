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

        }

        private void FilmBacklogView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void MusicBacklogView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void TVBacklogView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void GamesBacklogView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void BooksBacklogView_ItemClick(object sender, ItemClickEventArgs e)
        {

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
                EmtpyListText.Visibility = Visibility.Collapsed;
                switch(TypeComoBox.SelectedItem.ToString())
                {
                    case "Film":
                        await CreateFilmBacklog(NameInput.Text.Replace(" ", string.Empty), DatePicker.Date.ToString("D"));
                        EmtpyFilmsText.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private async Task CreateFilmBacklog(string title, string date)
        {
            string response = await RestClient.GetFilmResponse(title);
            FilmResult filmResult = JsonConvert.DeserializeObject<FilmResult>(response);
            FilmResponse filmResponse = filmResult.results[0];
            string filmData = await RestClient.GetFilmDataResponse(filmResponse.id);
            Debug.WriteLine(filmData);
            if (filmData != null)
            {
                Debug.WriteLine(filmData);
                Film film = JsonConvert.DeserializeObject<Film>(filmData);
                Backlog backlog = new Backlog
                {
                    id = new Guid(),
                    Name = film.fullTitle,
                    Type = "Film",
                    ReleaseDate = film.releaseDate,
                    ImageURL = film.image,
                    TargetDate = date,
                    Description = film.plot,
                    Length = film.runtimeStr,
                    Director = film.directors,
                };
                backlogs.Add(backlog);
                filmBacklogs.Add(backlog);
            }
        }

        private async Task CreateSeriesBacklog(string titile, string date)
        {
            string response = await RestClient.GetSeriesResponse(titile);
            TVResult tvResult = JsonConvert.DeserializeObject<TVResult>(response);
        }
    }
}
