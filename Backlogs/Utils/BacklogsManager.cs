using Backlogs.Models;
using Backlogs.Services;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Backlogs.Utils
{
    public class BacklogsManager
    {
        private static BacklogsManager m_instance = new BacklogsManager();
        private IFileHandler m_fileHandler;

        private ObservableCollection<Backlog> Backlogs = null;
        private ObservableCollection<Backlog> CompletedBacklogs = null;
        private ObservableCollection<Backlog> IncompleteBacklogs = null;
        private ObservableCollection<Backlog> RecentlyAddedBacklogs = null;
        private ObservableCollection<Backlog> RecentlyCompletedBacklogs = null;
        private ObservableCollection<Backlog> InProgressBacklogs = null;
        private ObservableCollection<Backlog> UpcomingBacklogs = null;

        public void InitBacklogsManager(IFileHandler fileHandler)
        {
            m_fileHandler = fileHandler;
        }

        public static BacklogsManager GetInstance()
        {
            return m_instance;
        }

        public async Task WriteDataAsync(bool sync = false)
        {
            Debug.WriteLine(m_fileHandler);
            await m_fileHandler.WriteBacklogsAsync(Backlogs, sync);
        }

        public void SaveSettings(ObservableCollection<Backlog> backlogs)
        {
            Backlogs = backlogs;
            ResetHelperBacklogs();
        }

        /// <summary>
        /// Read the backlog list in JSON and deserialze it 
        /// </summary>
        /// <returns></returns>
        public async Task ReadDataAsync(bool sync = false)
        {
            try
            {
                string json = await m_fileHandler.ReadBacklogsAsync(sync);
                Backlogs = JsonConvert.DeserializeObject<ObservableCollection<Backlog>>(json);
                Debug.WriteLine(json);
            }
            catch
            {
                Debug.WriteLine("Backlogs null");
                Backlogs = new ObservableCollection<Backlog>();
            }
        }

        public ObservableCollection<Backlog> GetBacklogs()
        {
            return Backlogs;
        }

        public ObservableCollection<Backlog> GetCompletedBacklogs()
        {
            return CompletedBacklogs;
        }

        public void ResetHelperBacklogs()
        {
            CompletedBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete));
            IncompleteBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete == false));
            InProgressBacklogs = new ObservableCollection<Backlog>(IncompleteBacklogs.Where(b => b.Progress > 0));
            var _sortedCompletedBacklogs = new ObservableCollection<Backlog>();
            var _sortedIncompleteBacklogs = new ObservableCollection<Backlog>();
            RecentlyAddedBacklogs = new ObservableCollection<Backlog>();
            RecentlyCompletedBacklogs = new ObservableCollection<Backlog>();
            UpcomingBacklogs = new ObservableCollection<Backlog>();
            foreach (var backlog in IncompleteBacklogs)
            {
                if (backlog.CreatedDate == "None" || backlog.CreatedDate == null)
                {
                    backlog.CreatedDate = DateTimeOffset.MinValue.ToString("d", CultureInfo.InvariantCulture);
                }
                _sortedIncompleteBacklogs.Add(backlog);
                try
                {
                    if (DateTime.Parse(backlog.TargetDate) >= DateTime.Today)
                    {
                        UpcomingBacklogs.Add(backlog);
                    }
                }
                catch
                {
                    continue;
                }
            }
            foreach (var backlog in CompletedBacklogs)
            {
                if (backlog.CompletedDate == null)
                {
                    backlog.CompletedDate = DateTimeOffset.MinValue.ToString("d", CultureInfo.InvariantCulture);
                }
                _sortedCompletedBacklogs.Add(backlog);
            }
            foreach (var backlog in _sortedIncompleteBacklogs.OrderByDescending(b => DateTime.Parse(b.CreatedDate, CultureInfo.InvariantCulture)).Skip(0).Take(6))
            {
                RecentlyAddedBacklogs.Add(backlog);
            }
            foreach (var backlog in _sortedCompletedBacklogs.OrderByDescending(b => DateTime.Parse(b.CompletedDate, CultureInfo.InvariantCulture)).Skip(0).Take(6))
            {
                RecentlyCompletedBacklogs.Add(backlog);
            }
        }

        public ObservableCollection<Backlog> GetIncompleteBacklogs()
        {
            return IncompleteBacklogs != null ? IncompleteBacklogs : new ObservableCollection<Backlog>();
        }

        public ObservableCollection<Backlog> GetRecentlyAddedBacklogs()
        {
            return RecentlyAddedBacklogs != null ? RecentlyAddedBacklogs : new ObservableCollection<Backlog>();
        }

        public ObservableCollection<Backlog> GetRecentlyCompletedBacklogs()
        {
            return RecentlyCompletedBacklogs != null ? RecentlyCompletedBacklogs : new ObservableCollection<Backlog>();
        }

        public ObservableCollection<Backlog> GetInProgressBacklogs()
        {
            return InProgressBacklogs != null ? InProgressBacklogs : new ObservableCollection<Backlog>();
        }

        public ObservableCollection<Backlog> GetUpcomingBacklogs()
        {
            return UpcomingBacklogs != null ? UpcomingBacklogs : new ObservableCollection<Backlog>();
        }
    }
}
