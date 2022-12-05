using Backlogs.Constants;
using Backlogs.Models;
using Backlogs.Services;
using Backlogs.Utils;
using MvvmHelpers.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Backlogs.Core.ViewModels
{
    public class CompletedBacklogViewModel : INotifyPropertyChanged
    {
        private bool m_isLoading;
        private readonly INavigation m_navigator;
        private readonly IUserSettings m_settings;
        private int m_backlogIndex;

        public ObservableCollection<Backlog> Backlogs;
        public Backlog Backlog;

        public ICommand SaveChanges { get; }
        public ICommand MarkAsIncomplete { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
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

        public CompletedBacklogViewModel(Guid id, INavigation navigator, IUserSettings settings)
        {
            m_navigator= navigator;
            m_settings = settings;

            SaveChanges = new AsyncCommand(SaveAndCloseAsync);
            MarkAsIncomplete= new AsyncCommand(MarkAsIncompleteAsync);

            Backlogs = BacklogsManager.GetInstance().GetBacklogs();

            foreach(var b in Backlogs)
            {
                if(id == b.id)
                {
                    Backlog = b;
                    m_backlogIndex = Backlogs.IndexOf(b);
                }
            }
        }

        /// <summary>
        /// Save the backlog and close it
        /// </summary>
        /// <returns></returns>
        private async Task SaveAndCloseAsync()
        {
            IsLoading = true;
            Backlogs[m_backlogIndex] = Backlog;
            BacklogsManager.GetInstance().SaveSettings(Backlogs);
            await BacklogsManager.GetInstance().WriteDataAsync(m_settings.Get<bool>(SettingsConstants.IsSignedIn));
            GoToPrevPage();
        }   
        
        /// <summary>
        /// Mark backlog as incomplete
        /// </summary>
        /// <returns></returns>
        private async Task MarkAsIncompleteAsync()
        {
            IsLoading = true;
            Backlog.IsComplete = false;
            Backlog.CompletedDate = null;
            await SaveAndCloseAsync();
        }

        private void GoToPrevPage()
        {
            m_navigator.GoBack();
        }
    }
}
