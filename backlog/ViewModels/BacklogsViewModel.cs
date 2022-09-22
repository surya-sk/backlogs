using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls;

namespace backlog.ViewModels
{
    public class BacklogsViewModel: INotifyPropertyChanged
    {
        private string _sortOrder = Settings.SortOrder;
        private bool _allEmpty;
        private bool _filmsEmpty;
        private bool _albumsEmpty;
        private bool _booksEmpty;
        private bool _tvEmpty;
        private bool _gamesEmpty;

        private BitmapImage _accountPic;

        public ObservableCollection<Backlog> Backlogs { get; set; }
        public ObservableCollection<Backlog> IncompleteBacklogs { get; set; }
        public ObservableCollection<Backlog> FilmBacklogs { get; set; }
        public ObservableCollection<Backlog> TvBacklogs { get; set; }
        public ObservableCollection<Backlog> GameBacklogs { get; set; }
        public ObservableCollection<Backlog> MusicBacklogs { get; set; }
        public ObservableCollection<Backlog> BookBacklogs { get; set; }

        private bool _isLoading;

        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void NavigateToBacklogsPage(Guid id);

        public ICommand SortByName { get; }
        public ICommand SortByCreatedDateAsc { get; }
        public ICommand SortByCreatedDateDsc { get; }
        public ICommand SortByProgressAsc { get; }
        public ICommand SortByProgressDsc { get; }
        public ICommand SortByTargetDateAsc { get; }
        public ICommand SortByTargetDateDsc { get; }
        public ICommand GenerateRandomBacklog { get; }

        public NavigateToBacklogsPage NavigateToBacklogsPageFunc { get; set; }

        public string UserName { get; } = Settings.UserName;
        public bool SignedIn { get; } = Settings.IsSignedIn;
        public bool ShowSignInButton { get; } = !Settings.IsSignedIn;

        public string SortOrder
        {
            get => _sortOrder;
            set
            {
                if (value != _sortOrder)
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlbumsEmpty)));
            }
        }

        public BitmapImage AccountPic
        {
            get => _accountPic;
            set
            {
                _accountPic = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountPic)));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }

        public BacklogsViewModel()
        {
            SortByName = new Command(SortBacklogsByName);
            SortByCreatedDateAsc = new Command(SortBacklogsByCreatedDateAsc);
            SortByCreatedDateDsc = new Command(SortBacklogsByCreatedDateDsc);
            SortByProgressAsc = new Command(SortBacklogsByProgressAsc);
            SortByProgressDsc = new Command(SortBacklogsByProgressDsc);
            SortByTargetDateAsc = new Command(SortBacklogsByTargetDateAsc);
            SortByTargetDateDsc = new Command(SortBacklogsByTargetDateDsc);
            GenerateRandomBacklog = new AsyncCommand<int>(GenerateRandomBacklogAsync);

            InitBacklogs();
            PopulateBacklogs();
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


        /// <summary>
        /// Populate the backlogs list with up-to-date backlogs
        /// </summary>
        public void PopulateBacklogs()
        {
            IncompleteBacklogs = SaveData.GetInstance().GetIncompleteBacklogs();
            ObservableCollection<Backlog> _backlogs = null;
            switch (SortOrder)
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
        public async Task SetUserPhotoAsync()
        {
            var cacheFolder = ApplicationData.Current.LocalCacheFolder;
            try
            {
                var accountPicFile = await cacheFolder.GetFileAsync("profile.png");
                using (IRandomAccessStream stream = await accountPicFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage image = new BitmapImage();
                    stream.Seek(0);
                    await image.SetSourceAsync(stream);
                    AccountPic = image;
                }
            }
            catch
            {
                // No image set
            }
        }

        /// <summary>
        /// Generates a random backlog of the selected type
        /// </summary>
        /// <returns></returns>
        private async Task GenerateRandomBacklogAsync(int pivotIndex)
        {
            Backlog randomBacklog = new Backlog();
            Random random = new Random();
            bool error = false;
            switch (pivotIndex)
            {
                case 0:
                    if (IncompleteBacklogs.Count < 2)
                    {
                        await ShowErrorMessageAsync("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = IncompleteBacklogs[random.Next(0, IncompleteBacklogs.Count)];
                    break;
                case 1:
                    if (FilmBacklogs.Count < 2)
                    {
                        await ShowErrorMessageAsync("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = FilmBacklogs[random.Next(0, FilmBacklogs.Count)];
                    break;
                case 2:
                    if (MusicBacklogs.Count < 2)
                    {
                        await ShowErrorMessageAsync("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = MusicBacklogs[random.Next(0, MusicBacklogs.Count)];
                    break;
                case 3:
                    if (TvBacklogs.Count < 2)
                    {
                        await ShowErrorMessageAsync("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = TvBacklogs[random.Next(0, TvBacklogs.Count)];
                    break;
                case 4:
                    if (GameBacklogs.Count < 2)
                    {
                        await ShowErrorMessageAsync("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = GameBacklogs[random.Next(0, GameBacklogs.Count)];
                    break;
                case 5:
                    if (BookBacklogs.Count < 2)
                    {
                        await ShowErrorMessageAsync("Add backlogs to get random picks");
                        error = true;
                        break;
                    }
                    randomBacklog = BookBacklogs[random.Next(0, BookBacklogs.Count)];
                    break;
            }

            if (!error)
            {
                await ShowRandomPickAsync(randomBacklog, pivotIndex);
            }
        }

        private async Task ShowErrorMessageAsync(string message)
        {
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = "Not enough Backlogs",
                Content = message,
                CloseButtonText = "Ok"
            };
            await contentDialog.ShowAsync();
        }

        private async Task ShowRandomPickAsync(Backlog backlog, int index)
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
            if (result == ContentDialogResult.Primary)
            {
                await GenerateRandomBacklogAsync(index);
            }
            else if (result == ContentDialogResult.Secondary)
            {
                NavigateToBacklogsPageFunc(backlog.id);
            }
        }

        #region Sorting
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
        #endregion
    }
}
