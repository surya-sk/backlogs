using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backlogs.Models
{
    public class Backlog : INotifyPropertyChanged
    {
        public Guid id { get; set; }
        public string API_ID { get; set; }
        private string m_name;
        private string m_type;
        private string m_description;
        private string m_releaseDate;
        private string m_targetDate;
        private bool m_isComplete;
        private string m_imageURL;
        private string m_trailerURL;
        public int m_progress;
        private int m_length; 
        private string m_searchURL;
        private string m_units;
        private string m_director;
        private string m_ratings;
        private List<SearchResult> m_similar;
        private Dictionary<string, string> m_links;
        private List<string> m_genres;
        private bool m_showProgress;
        private TimeSpan m_notifTime;
        private bool m_remindEveryday;
        private double m_userRating;
        private string m_createdDate;
        private string m_completedDate;

        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public string Type
        {
            get => m_type;
            set
            {
                m_type = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
            }
        }

        public string Description
        {
            get => m_description;
            set
            {
                m_description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        public string ReleaseDate
        {
            get => m_releaseDate;
            set
            {
                m_releaseDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReleaseDate)));
            }
        }

        public string Ratings
        {
            get => m_ratings;
            set
            {
                m_ratings = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof (Ratings)));
            }
        }

        public string TargetDate
        {
            get => m_targetDate;
            set
            {
                m_targetDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetDate)));
            }
        }

        public string Units
        {
            get => m_units;
            set
            {
                m_units = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Units)));
            }
        }

        public Dictionary<string, string> Links
        {
            get => m_links;
            set
            {
                m_links = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Links)));
            }
        }

        public List<string> Genres
        {
            get => m_genres;
            set
            {
                m_genres = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Genres)));
            }
        }

        public TimeSpan NotifTime
        {
            get => m_notifTime;
            set
            {
                m_notifTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotifTime)));
            }
        }

        public List<SearchResult> Similar
        {
            get => m_similar;
            set
            {
                m_similar = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Similar)));
            }
        }

        public bool IsComplete
        {
            get => m_isComplete;
            set
            {
                m_isComplete = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));
            }
        }

        public bool ShowProgress
        {
            get => m_showProgress;
            set
            {
                m_showProgress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowProgress)));
            }
        }

        public bool RemindEveryday
        {
            get => m_remindEveryday;
            set
            {
                m_remindEveryday = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemindEveryday)));
            }
        }

        public string ImageURL
        {
            get => m_imageURL;
            set
            {
                m_imageURL = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageURL)));
            }
        }

        public int Progress
        {
            get => m_progress;
            set
            {
                m_progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        public int Length
        {
            get => m_length;
            set
            {
                m_length = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Length)));
            }
        }

        public string TrailerURL
        {
            get => m_trailerURL;
            set
            {
                m_trailerURL = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrailerURL)));
            }
        }

        public string SearchURL
        {
            get => m_searchURL;
            set
            {
                m_searchURL = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchURL)));
            }
        }

        public string Director
        {
            get => m_director;
            set
            {
                m_director = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Director)));
            }
        }

        public double UserRating
        {
            get => m_userRating;
            set
            {
                m_userRating = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserRating)));
            }
        }

        public string CreatedDate
        {
            get => m_createdDate;
            set
            {
                m_createdDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreatedDate)));
            }
        }

        public string CompletedDate
        {
            get => m_completedDate;
            set
            {
                m_completedDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompletedDate)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
