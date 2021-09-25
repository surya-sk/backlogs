using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backlog.Models
{
    public class Backlog : INotifyPropertyChanged
    {
        private string name;
        private string type;
        private string description;
        private string releaseDate;
        private string targetDate;
        private bool hasReleased;
        private string imageURL;
        private string thirdPartyURL;
        private string trailerURL;
        private string searchURL;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public string Type
        {
            get => type;
            set
            {
                type = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
            }
        }

        public string Description
        {
            get => description;
            set
            {
                description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        public string ReleaseDate
        {
            get => releaseDate;
            set
            {
                releaseDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReleaseDate)));
            }
        }

        public string TargetDate
        {
            get => targetDate;
            set
            {
                targetDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetDate)));
            }
        }

        public bool HasReleased
        {
            get => hasReleased;
            set
            {
                hasReleased = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasReleased)));
            }
        }

        public string ImageURL
        {
            get => imageURL;
            set
            {
                imageURL = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageURL)));
            }
        }

        public string ThirdPartyURL
        {
            get => thirdPartyURL;
            set
            {
                thirdPartyURL = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThirdPartyURL)));
            }
        }

        public string TrailerURL
        {
            get => trailerURL;
            set
            {
                trailerURL = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrailerURL)));
            }
        }

        public string SearchURL
        {
            get => searchURL;
            set
            {
                searchURL = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchURL)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
