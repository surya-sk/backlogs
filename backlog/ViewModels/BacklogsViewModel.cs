using Backlogs.Models;
using MvvmHelpers.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Backlogs.Logging;
using System.Net.NetworkInformation;
using Microsoft.Toolkit.Uwp;
using Backlogs.Services;
using Backlogs.Utils.Core;
using Backlogs.Constants;

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

        private string m_accountPic;

        private bool m_isLoading;
        private ObservableCollection<Backlog> m_filmBacklogs;
        private ObservableCollection<Backlog> m_tvBacklogs;
        private ObservableCollection<Backlog> m_gameBacklogs;
        private ObservableCollection<Backlog> m_musicBacklogs;
        private ObservableCollection<Backlog> m_bookBacklogs;
        private ObservableCollection<Backlog> m_incompleteBacklogs;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Backlog> Backlogs { get; set; }
        public IncrementalLoadingCollection<BacklogSource, Backlog> IncompleteBacklogs { get; set; }
        public IncrementalLoadingCollection<BacklogSource, Backlog> FilmBacklogs { get; set; }
        public IncrementalLoadingCollection<BacklogSource, Backlog> TvBacklogs { get; set; }
        public IncrementalLoadingCollection<BacklogSource, Backlog> GameBacklogs { get; set; }
        public IncrementalLoadingCollection<BacklogSource, Backlog> MusicBacklogs { get; set; }
        public IncrementalLoadingCollection<BacklogSource, Backlog> BookBacklogs { get; set; }

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
                if (value != m_sortOrder)
                {
                    m_sortOrder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SortOrder)));
                    m_settings.Set(SettingsConstants.SortOrder, value);
                    PopulateBacklogs();
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

        public string AccountPic
        {
            get => m_accountPic;
            set
            {
                m_accountPic = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountPic)));
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

            PopulateBacklogs();
            m_settings = settings;
        }

        public async Task SyncBacklogs(bool sync)
        {
            IsLoading = true;
            if (NetworkInterface.GetIsNetworkAvailable() && SignedIn)
            {
                if (sync)
                {
                    await SetUserPhotoAsync();
                    try
                    {
                        await Logger.Info("Syncing backlogs....");
                    }
                    catch { }
                }
            }
            CheckEmptyBacklogs();
            IsLoading = false;
        }


        /// <summary>
        /// Populate the backlogs list with up-to-date backlogs
        /// </summary>
        public void PopulateBacklogs()
        {
            m_incompleteBacklogs = BacklogsManager.GetInstance().GetIncompleteBacklogs();
            ObservableCollection<Backlog> _backlogs = null;
            switch (SortOrder)
            {
                case "Name":
                    _backlogs = new ObservableCollection<Backlog>(m_incompleteBacklogs.OrderBy(b => b.Name));
                    break;
                case "Created Date Asc.":
                    _backlogs = new ObservableCollection<Backlog>(m_incompleteBacklogs.OrderBy(b => Convert.ToDateTime(b.CreatedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Created Date Dsc.":
                    _backlogs = new ObservableCollection<Backlog>(m_incompleteBacklogs.OrderByDescending(b => Convert.ToDateTime(b.CreatedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Target Date Asc.":
                    _backlogs = new ObservableCollection<Backlog>(m_incompleteBacklogs.OrderBy(b => Convert.ToDateTime(b.TargetDate, CultureInfo.InvariantCulture)));
                    break;
                case "Target Date Dsc.":
                    _backlogs = new ObservableCollection<Backlog>(m_incompleteBacklogs.OrderByDescending(b => Convert.ToDateTime(b.TargetDate, CultureInfo.InvariantCulture)));
                    break;
                case "Progress Asc.":
                    _backlogs = new ObservableCollection<Backlog>(m_incompleteBacklogs.OrderBy(b => b.Progress));
                    break;
                case "Progress Dsc.":
                    _backlogs = new ObservableCollection<Backlog>(m_incompleteBacklogs.OrderByDescending(b => b.Progress));
                    break;
            }
            m_filmBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Film.ToString()));
            m_tvBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.TV.ToString()));
            m_gameBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Game.ToString()));
            m_musicBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Album.ToString()));
            m_bookBacklogs = new ObservableCollection<Backlog>(_backlogs.Where(b => b.Type == BacklogType.Book.ToString()));

            IncompleteBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(m_incompleteBacklogs));
            FilmBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(m_filmBacklogs));
            TvBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(m_tvBacklogs));
            MusicBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(m_musicBacklogs));
            GameBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(m_gameBacklogs));
            BookBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(m_bookBacklogs));
        }

        private void CheckEmptyBacklogs()
        {
            BacklogsEmpty = m_incompleteBacklogs.Count <= 0;
            FilmsEmpty = m_filmBacklogs.Count <= 0;
            BooksEmpty = m_bookBacklogs.Count <= 0;
            TVEmpty = m_tvBacklogs.Count <= 0;
            GamesEmpty = m_gameBacklogs.Count <= 0;
            AlbumsEmpty = m_musicBacklogs.Count <= 0;
        }

        /// <summary>
        /// Set the user photo in the command bar
        /// </summary>
        /// <returns></returns>
        public async Task SetUserPhotoAsync()
        {
            try
            {
                AccountPic = await m_fileHandler.ReadImageAsync("profile.png");
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
            m_navigationService.NavigateTo<BacklogsViewModel>("sync");
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
