using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using Microsoft.Toolkit.Uwp.UI.Controls;
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

namespace backlog.ViewModels
{

    public class CompletedBacklogsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Backlog> FinishedBacklogs;
        public ObservableCollection<Backlog> FinishedFilmBacklogs;
        public ObservableCollection<Backlog> FinishedTVBacklogs;
        public ObservableCollection<Backlog> FinishedMusicBacklogs;
        public ObservableCollection<Backlog> FinishedGameBacklogs;
        public ObservableCollection<Backlog> FinishedBookBacklogs;
        public ObservableCollection<Backlog> Backlogs;
        public Backlog SelectedBacklog;
        private string _sortOrder = Settings.CompletedSortOrder;
        private bool _loading = false;
        private double _userRating;

        public delegate Task CloseBacklogFunc();
        public delegate void ClosePopupFunc();
        public event PropertyChangedEventHandler PropertyChanged;

        public CloseBacklogFunc CloseBacklog;
        public ClosePopupFunc ClosePopup;

        public ICommand SaveBacklog { get; }
        public ICommand MarkBacklogAsIncomplete { get; }
        public ICommand SortByName { get; }
        public ICommand SortByDateDsc { get; }
        public ICommand SortByDateAsc { get; }
        public ICommand SortByRatingDsc { get; }
        public ICommand SortByRatingAsc { get; }


        public double UserRating
        {
            get => _userRating;
            set
            {
                _userRating = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserRating)));
            }
        }

        public bool IsLoading
        {
            get => _loading;
            set
            {
                _loading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }

        public string SortOrder
        {
            get => _sortOrder;
            set
            {
                if (value != _sortOrder)
                {
                    _sortOrder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SortOrder)));
                    Settings.CompletedSortOrder = _sortOrder;
                    PopulateBacklogs();
                }
            }
        }

        public CompletedBacklogsViewModel()
        {
            SaveBacklog = new AsyncCommand(SaveBacklogAsync);
            MarkBacklogAsIncomplete = new AsyncCommand(MarkBacklogAsIncompleteAsync);
            SortByName = new Command(SortBacklogsByName);
            SortByDateAsc = new Command(SortBacklogsByCompletedDateAsc);
            SortByDateDsc = new Command(SortBacklogsByCompletedDateDsc);
            SortByRatingAsc = new Command(SortBacklogsByRatingsAsc);
            SortByRatingDsc = new Command(SortBacklogsByRatingDsc);
            //CloseBacklog = CloseBacklogAsync;
            //ClosePopup = ClosePopupOverlayAndReload;

            Backlogs = SaveData.GetInstance().GetBacklogs();
            FinishedBacklogs = SaveData.GetInstance().GetCompletedBacklogs();
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
            FinishedBacklogs = SaveData.GetInstance().GetCompletedBacklogs();
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
            SaveData.GetInstance().SetCompletedBacklogs(FinishedBacklogs);
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
            SaveData.GetInstance().SetCompletedBacklogs(FinishedBacklogs);
            await SaveData.GetInstance().WriteDataAsync(Settings.IsSignedIn);
            IsLoading = false;
            await CloseBacklog();
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
            SaveData.GetInstance().SetCompletedBacklogs(FinishedBacklogs);
            await SaveData.GetInstance().WriteDataAsync(Settings.IsSignedIn);
            ClosePopup();
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
