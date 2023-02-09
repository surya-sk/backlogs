using Backlogs.Models;
using MvvmHelpers.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Net.NetworkInformation;
using Backlogs.Services;
using Backlogs.Utils;
using Backlogs.Constants;
using System.Diagnostics;

namespace Backlogs.ViewModels
{
    public class BacklogsViewModel: INotifyPropertyChanged
    {
        private string m_sortOrder;
        private bool m_allEmpty;
        private bool m_filmsEmpty;
        private bool m_albumsEmpty;
        private bool m_booksEmpty;
        private bool m_tvEmpty;
        private bool m_gamesEmpty;
        private readonly INavigation m_navigationService;
        private readonly IDialogHandler m_dialogHandler;
        private readonly IFileHandler m_fileHandler;
        private IUserSettings m_settings;

        private bool m_isLoading;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Backlog> Backlogs { get; set; }
        public ObservableCollection<Backlog> IncompleteBacklogs { get; set; }
        public ObservableCollection<Backlog> FilmBacklogs { get; set; }
        public ObservableCollection<Backlog> TvBacklogs { get; set; }
        public ObservableCollection<Backlog> GameBacklogs { get; set; }
        public ObservableCollection<Backlog> MusicBacklogs { get; set; }
        public ObservableCollection<Backlog> BookBacklogs { get; set; }

        public ICommand SortByName { get; }
        public ICommand SortByCreatedDateAsc { get; }
        public ICommand SortByCreatedDateDsc { get; }
        public ICommand SortByProgressAsc { get; }
        public ICommand SortByProgressDsc { get; }
        public ICommand SortByTargetDateAsc { get; }
        public ICommand SortByTargetDateDsc { get; }
        public ICommand GenerateRandomBacklog { get; }
        public ICommand OpenCompletedBacklogs { get; }
        public ICommand OpenSettings { get; }
        public ICommand Reload { get; }
        public ICommand OpenCreatePage { get; }

        public string UserName { get => m_settings.Get<string>(SettingsConstants.UserName); }
        public bool SignedIn { get => m_settings.Get<bool>(SettingsConstants.IsSignedIn); } 
        public bool ShowSignInButton { get => !SignedIn; }  

        #region Properties
        public string SortOrder
        {
            get
            {
                m_sortOrder = m_settings.Get<string>(SettingsConstants.SortOrder);
                return m_sortOrder;
            }
            set
            {
                if(m_sortOrder != value)
                {
                    m_sortOrder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SortOrder)));
                    m_settings.Set(SettingsConstants.SortOrder, value);
                    ReloadAndSync();
                }
            }
        }

        public bool BacklogsEmpty
        {
            get => m_allEmpty;
            set
            {
                m_allEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BacklogsEmpty)));
            }
        }

        public bool FilmsEmpty
        {
            get => m_filmsEmpty;
            set
            {
                m_filmsEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilmsEmpty)));
            }
        }

        public bool TVEmpty
        {
            get => m_tvEmpty;
            set
            {
                m_tvEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TVEmpty)));
            }
        }

        public bool BooksEmpty
        {
            get => m_booksEmpty;
            set
            {
                m_booksEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BooksEmpty)));
            }
        }

        public bool GamesEmpty
        {
            get => m_gamesEmpty;
            set
            {
                m_gamesEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GamesEmpty)));
            }
        }

        public bool AlbumsEmpty
        {
            get => m_albumsEmpty;
            set
            {
                m_albumsEmpty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlbumsEmpty)));
            }
        }

        public bool IsLoading
        {
            get => m_isLoading;
            set
            {
                m_isLoading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }
        #endregion

        public BacklogsViewModel(INavigation navigationService, IDialogHandler dialogHandler, IFileHandler fileHandler, IUserSettings settings)
        {
            SortByName = new Command(SortBacklogsByName);
            SortByCreatedDateAsc = new Command(SortBacklogsByCreatedDateAsc);
            SortByCreatedDateDsc = new Command(SortBacklogsByCreatedDateDsc);
            SortByProgressAsc = new Command(SortBacklogsByProgressAsc);
            SortByProgressDsc = new Command(SortBacklogsByProgressDsc);
            SortByTargetDateAsc = new Command(SortBacklogsByTargetDateAsc);
            SortByTargetDateDsc = new Command(SortBacklogsByTargetDateDsc);
            GenerateRandomBacklog = new AsyncCommand<int>(GenerateRandomBacklogAsync);
            OpenCompletedBacklogs = new Command(OpenCompletedPage);
            OpenSettings = new Command(OpenSettingsPage);
            Reload = new Command(ReloadAndSync);
            OpenCreatePage = new Command(NavigateToCreatePage);

            m_navigationService = navigationService;
            m_dialogHandler = dialogHandler;
            m_fileHandler = fileHandler;
            m_settings = settings;

            InitBacklogs();
            PopulateBacklogs();
        }

        public async Task SyncBacklogs(bool sync)
        {
            IsLoading = true;
            if (NetworkInterface.GetIsNetworkAvailable() && SignedIn)
            {
                if (sync)
                {
                    try
                    {
                        await m_fileHandler.WriteLogsAsync("Syncing backlogs....");
                    }
                    catch { }
                }
            }
            CheckEmptyBacklogs();
            IsLoading = false;
        }
        /// <summary>
        /// Initalize backlogs
        /// </summary>
        private void InitBacklogs()
        {
            IncompleteBacklogs = BacklogsManager.GetInstance().GetIncompleteBacklogs();
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
            Debug.WriteLine("Hello");
            IncompleteBacklogs = BacklogsManager.GetInstance().GetIncompleteBacklogs();
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

        /// <summary>
        /// Popup an error message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task ShowErrorMessageAsync(string message)
        {
            await m_dialogHandler.ShowErrorDialogAsync("Not enough backlogs", message, "Ok");

        }

        /// <summary>
        /// Show a random backlog
        /// </summary>
        /// <param name="backlog"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task ShowRandomPickAsync(Backlog backlog, int index)
        {
            var result = await m_dialogHandler.ShowRandomBacklogDialogAsync(backlog);
            if (result == 1)
            {
                await GenerateRandomBacklogAsync(index);
            }
            else if (result == 2)
            {
                m_navigationService.NavigateTo<BacklogViewModel>(backlog.id);
            }
        }

        private void OpenCompletedPage()
        {
             m_navigationService.NavigateTo<CompletedBacklogsViewModel>();
        }

        private void OpenSettingsPage()
        {
            m_navigationService.NavigateTo<SettingsViewModel>();
        }

        private void ReloadAndSync()
        {
            m_navigationService.NavigateTo<BacklogsViewModel>();
        }

        private void NavigateToCreatePage(object typeIndex)
        {
            
            m_navigationService.NavigateTo<CreateBacklogViewModel>(typeIndex);
        }

        public void GoBack()
        {
            m_navigationService.NavigateTo<MainViewModel>();
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
