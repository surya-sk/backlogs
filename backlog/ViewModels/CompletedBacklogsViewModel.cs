using Backlogs.Models;
using Backlogs.Saving;
using Backlogs.Utils;
using Microsoft.Toolkit.Uwp;
using MvvmHelpers.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Backlogs.ViewModels
{
    public class CompletedBacklogsViewModel : INotifyPropertyChanged
    {
        private string m_sortOrder = Settings.CompletedSortOrder;
        private bool m_loading = false;
        private double m_userRating;
        private bool m_allEmpty;
        private bool m_filmsEmpty;
        private bool m_albumsEmpty;
        private bool m_booksEmpty;
        private bool m_tvEmpty;
        private bool m_gamesEmpty;
        private ContentDialog m_popupOverlay;
        private readonly INavigationService m_navigationService;

        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedBacklogs;
        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedFilmBacklogs;
        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedTVBacklogs;
        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedMusicBacklogs;
        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedGameBacklogs;
        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedBookBacklogs;
        public ObservableCollection<Backlog> Backlogs;
        public Backlog SelectedBacklog;

        public delegate Task PlayConnectionAnimation();

        public PlayConnectionAnimation PlayConnectionAnimationAsync;
        private ObservableCollection<Backlog> m_finishedBacklogs;
        private ObservableCollection<Backlog> _finishedBookBacklogs;
        private ObservableCollection<Backlog> _finishedFilmBacklogs;
        private ObservableCollection<Backlog> _finishedGameBacklogs;
        private ObservableCollection<Backlog> _finishedMusicBacklogs;
        private ObservableCollection<Backlog> _finishedTVBacklogs;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SaveBacklog { get; }
        public ICommand MarkBacklogAsIncomplete { get; }
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
            get => m_sortOrder;
            set
            {
                if (value != m_sortOrder)
                {
                    m_sortOrder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SortOrder)));
                    Settings.CompletedSortOrder = m_sortOrder;
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

        public CompletedBacklogsViewModel(ContentDialog popupOverlay, INavigationService navigationService)
        {
            SaveBacklog = new AsyncCommand(SaveBacklogAsync);
            MarkBacklogAsIncomplete = new AsyncCommand(MarkBacklogAsIncompleteAsync);
            SortByName = new Command(SortBacklogsByName);
            SortByDateAsc = new Command(SortBacklogsByCompletedDateAsc);
            SortByDateDsc = new Command(SortBacklogsByCompletedDateDsc);
            SortByRatingAsc = new Command(SortBacklogsByRatingsAsc);
            SortByRatingDsc = new Command(SortBacklogsByRatingDsc);
            Reload = new Command(ReloadPage);
            OpenSettings = new Command(NavigateToSettingsPage);
            CloseBacklog = new AsyncCommand(CloseBacklogAsync);

            m_popupOverlay = popupOverlay;
            m_navigationService = navigationService;

            Backlogs = SaveData.GetInstance().GetBacklogs();
            PopulateBacklogs();
        }

        /// <summary>
        /// Populate all backlog categories
        /// </summary>
        private void PopulateBacklogs()
        {
            m_finishedBacklogs = SaveData.GetInstance().GetCompletedBacklogs();
            ObservableCollection<Backlog> _finishedBacklogs = null;
            switch (SortOrder)
            {
                case "Name":
                    _finishedBacklogs = new ObservableCollection<Backlog>(m_finishedBacklogs.OrderBy(b => b.Name));
                    break;
                case "Completed Date Asc.":
                    _finishedBacklogs = new ObservableCollection<Backlog>(m_finishedBacklogs.OrderBy(b => Convert.ToDateTime(b.CompletedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Completed Date Dsc.":
                    _finishedBacklogs = new ObservableCollection<Backlog>(m_finishedBacklogs.OrderByDescending(b => Convert.ToDateTime(b.CompletedDate, CultureInfo.InvariantCulture)));
                    break;
                case "Lowest Rating":
                    _finishedBacklogs = new ObservableCollection<Backlog>(m_finishedBacklogs.OrderBy(b => b.UserRating));
                    break;
                case "Highest Rating":
                    _finishedBacklogs = new ObservableCollection<Backlog>(m_finishedBacklogs.OrderByDescending(b => b.UserRating));
                    break;

            }
            _finishedBookBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Book.ToString()));
            _finishedFilmBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Film.ToString()));
            _finishedGameBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Game.ToString()));
            _finishedMusicBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.Album.ToString()));
            _finishedTVBacklogs = new ObservableCollection<Backlog>(_finishedBacklogs.Where(b => b.Type == BacklogType.TV.ToString()));

            FinishedBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(m_finishedBacklogs));
            FinishedFilmBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(_finishedFilmBacklogs));
            FinishedGameBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(_finishedGameBacklogs));
            FinishedBookBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(_finishedBookBacklogs));
            FinishedTVBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(_finishedTVBacklogs));
            FinishedMusicBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(_finishedMusicBacklogs));
            
            CheckEmptyBacklogs();
        }

        private void CheckEmptyBacklogs()
        {
            BacklogsEmpty = m_finishedBacklogs.Count <= 0;
            FilmsEmpty = _finishedFilmBacklogs.Count <= 0;
            BooksEmpty = _finishedBookBacklogs.Count <= 0;
            TVEmpty = _finishedTVBacklogs.Count <= 0;
            GamesEmpty = _finishedGameBacklogs.Count <= 0;
            AlbumsEmpty = _finishedMusicBacklogs.Count <= 0;
        }

        /// <summary>
        /// Saves the backlog
        /// </summary>
        /// <returns></returns>
        private async Task SaveBacklogAsync()
        {
            IsLoading = true;
            foreach (var backlog in Backlogs)
            {
                if (backlog.id == SelectedBacklog.id)
                {
                    backlog.UserRating = UserRating;
                }
            }
            foreach (var backlog in FinishedBacklogs)
            {
                if (backlog.id == SelectedBacklog.id)
                {
                    backlog.UserRating = UserRating;
                }
            }
            SaveData.GetInstance().SaveSettings(Backlogs);
            await SaveData.GetInstance().WriteDataAsync(Settings.IsSignedIn);
            IsLoading = false;
            await CloseBacklogAsync();
        }

        private async Task CloseBacklogAsync()
        {
            try
            {
                m_popupOverlay.Hide();
                await PlayConnectionAnimationAsync();
            }
            catch
            {
                m_popupOverlay.Hide();
            }
        }

        /// <summary>
        /// Marks backlog as incomplete
        /// </summary>
        /// <returns></returns>
        private async Task MarkBacklogAsIncompleteAsync()
        {
            IsLoading = true;
            foreach (var backlog in Backlogs)
            {
                if (backlog.id == SelectedBacklog.id)
                {
                    backlog.IsComplete = false;
                    backlog.CompletedDate = null;
                }
            }
            SaveData.GetInstance().SaveSettings(Backlogs);
            await SaveData.GetInstance().WriteDataAsync(Settings.IsSignedIn);
            m_popupOverlay.Hide();
            ReloadPage();
        }

        private void ReloadPage()
        {
            m_navigationService.NavigateTo<CompletedBacklogsViewModel>();
        }

        private void NavigateToSettingsPage()
        {
            m_navigationService.NavigateTo<SettingsViewModel>();
        }

        public void GoBack(object sender, BackRequestedEventArgs e)
        {
            try
            {
                m_navigationService.GoBack(new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            catch
            {
                m_navigationService.GoBack();
            }
            e.Handled = true;
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
