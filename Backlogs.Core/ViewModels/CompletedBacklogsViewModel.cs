using Backlogs.Constants;
using Backlogs.Models;
using Backlogs.Services;
using Backlogs.Utils;
using MvvmHelpers.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Backlogs.ViewModels
{
    public class CompletedBacklogsViewModel : INotifyPropertyChanged
    {
        private string m_sortOrder;
        private bool m_loading = false;
        private double m_userRating;
        private bool m_allEmpty;
        private bool m_filmsEmpty;
        private bool m_albumsEmpty;
        private bool m_booksEmpty;
        private bool m_tvEmpty;
        private bool m_gamesEmpty;
        private readonly INavigation m_navigationService;
        private readonly IUserSettings m_settings;

        public ObservableCollection<Backlog> FinishedBacklogs;
        public ObservableCollection<Backlog> FinishedFilmBacklogs;
        public ObservableCollection<Backlog> FinishedTVBacklogs;
        public ObservableCollection<Backlog> FinishedMusicBacklogs;
        public ObservableCollection<Backlog> FinishedGameBacklogs;
        public ObservableCollection<Backlog> FinishedBookBacklogs;
        public ObservableCollection<Backlog> Backlogs;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SortByName { get; }
        public ICommand SortByDateDsc { get; }
        public ICommand SortByDateAsc { get; }
        public ICommand SortByRatingDsc { get; }
        public ICommand SortByRatingAsc { get; }
        public ICommand Reload { get; }
        public ICommand OpenSettings { get; }
        public ICommand CloseBacklog { get; }

        #region Properties
        public double UserRating
        {
            get => m_userRating;
            set
            {
                m_userRating = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserRating)));
            }
        }

        public bool IsLoading
        {
            get => m_loading;
            set
            {
                m_loading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }

        public string SortOrder
        {
            get
            {
                m_sortOrder = m_settings.Get<string>(SettingsConstants.CompletedSortOrder);
                return m_sortOrder;
            }
            set
            {
                if (value != m_sortOrder)
                {
                    m_sortOrder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SortOrder)));
                    m_settings.Set(SettingsConstants.CompletedSortOrder, value);
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
        #endregion

        public CompletedBacklogsViewModel(INavigation navigationService, IUserSettings settings)
        {
            SortByName = new Command(SortBacklogsByName);
            SortByDateAsc = new Command(SortBacklogsByCompletedDateAsc);
            SortByDateDsc = new Command(SortBacklogsByCompletedDateDsc);
            SortByRatingAsc = new Command(SortBacklogsByRatingsAsc);
            SortByRatingDsc = new Command(SortBacklogsByRatingDsc);
            Reload = new Command(ReloadPage);
            OpenSettings = new Command(NavigateToSettingsPage);

            m_navigationService = navigationService;
            m_settings = settings;

            Backlogs = BacklogsManager.GetInstance().GetBacklogs();
            FinishedBacklogs = BacklogsManager.GetInstance().GetCompletedBacklogs();
            FinishedBookBacklogs = new ObservableCollection<Backlog>();
            FinishedFilmBacklogs = new ObservableCollection<Backlog>();
            FinishedTVBacklogs = new ObservableCollection<Backlog>();
            FinishedMusicBacklogs = new ObservableCollection<Backlog>();
            FinishedGameBacklogs = new ObservableCollection<Backlog>();
            PopulateBacklogs();
        }

        /// <summary>
        /// Populate all backlog categories
        /// </summary>
        private void PopulateBacklogs()
        {
            FinishedBacklogs = BacklogsManager.GetInstance().GetCompletedBacklogs();
            ObservableCollection<Backlog> _finishedBacklogs = null;
            switch (SortOrder)
            {
                case "Name":
                    _finishedBacklogs = new ObservableCollection<Backlog>(FinishedBacklogs.OrderBy(b => b.Name));
                    break;
                case "Completed Date Asc.":
                    _finishedBacklogs = new ObservableCollection<Backlog>(FinishedBacklogs.OrderBy(b => Convert.ToDateTime(b.CompletedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Completed Date Dsc.":
                    _finishedBacklogs = new ObservableCollection<Backlog>(FinishedBacklogs.OrderByDescending(b => Convert.ToDateTime(b.CompletedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Lowest Rating":
                    _finishedBacklogs = new ObservableCollection<Backlog>(FinishedBacklogs.OrderBy(b => b.UserRating));
                    break;
                case "Highest Rating":
                    _finishedBacklogs = new ObservableCollection<Backlog>(FinishedBacklogs.OrderByDescending(b => b.UserRating));
                    break;

            }
            var _finishedBookBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Book.ToString()));
            var _finishedFilmBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Film.ToString()));
            var _finishedGameBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Game.ToString()));
            var _finishedMusicBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Album.ToString()));
            var _finishedTVBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.TV.ToString()));
            FinishedBacklogs.Clear();
            FinishedFilmBacklogs.Clear();
            FinishedTVBacklogs.Clear();
            FinishedMusicBacklogs.Clear();
            FinishedGameBacklogs.Clear();
            FinishedBookBacklogs.Clear();

            foreach (var backlog in _finishedBacklogs)
            {
                FinishedBacklogs.Add(backlog);
            }
            foreach (var backlog in _finishedBookBacklogs)
            {
                FinishedBookBacklogs.Add(backlog);
            }
            foreach (var backlog in _finishedFilmBacklogs)
            {
                FinishedFilmBacklogs.Add(backlog);
            }
            foreach (var backlog in _finishedGameBacklogs)
            {
                FinishedGameBacklogs.Add(backlog);
            }
            foreach (var backlog in _finishedMusicBacklogs)
            {
                FinishedMusicBacklogs.Add(backlog);
            }
            foreach (var backlog in _finishedTVBacklogs)
            {
                FinishedTVBacklogs.Add(backlog);
            }

            CheckEmptyBacklogs();
        }

        private void CheckEmptyBacklogs()
        {
            BacklogsEmpty = FinishedBacklogs.Count <= 0;
            FilmsEmpty = FinishedFilmBacklogs.Count <= 0;
            BooksEmpty = FinishedBookBacklogs.Count <= 0;
            TVEmpty = FinishedTVBacklogs.Count <= 0;
            GamesEmpty = FinishedGameBacklogs.Count <= 0;
            AlbumsEmpty = FinishedMusicBacklogs.Count <= 0;
        }

        private void ReloadPage()
        {
            m_navigationService.NavigateTo<CompletedBacklogsViewModel>();
        }

        private void NavigateToSettingsPage()
        {
            m_navigationService.NavigateTo<SettingsViewModel>();
        }

        public void GoBack()
        {
            m_navigationService.GoBack();
        }

        #region Sorting
        private void SortBacklogsByName()
        {
            SortOrder = "Name";
        }

        private void SortBacklogsByCompletedDateAsc()
        {
            SortOrder = "Completed Date Asc.";
        }

        private void SortBacklogsByCompletedDateDsc()
        {
            SortOrder = "Completed Date Dsc.";
        }

        private void SortBacklogsByRatingsAsc()
        {
            SortOrder = "Lowest Rating";
        }


        private void SortBacklogsByRatingDsc()
        {
            SortOrder = "Highest Rating";
        }
        #endregion
    }
}
