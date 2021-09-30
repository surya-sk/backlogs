﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backlog.Models
{
    public class Backlog : INotifyPropertyChanged
    {
        public Guid id { get; set; }
        private string name;
        private string type;
        private string description;
        private string releaseDate;
        private string targetDate;
        private bool hasReleased;
        private string imageURL;
        private string trailerURL;
        private string length; 
        private string searchURL;
        private string director;

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

        public string Length
        {
            get => length;
            set
            {
                length = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Length)));
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

        public string Director
        {
            get => director;
            set
            {
                director = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Director)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}