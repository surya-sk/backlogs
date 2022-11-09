using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using backlog.Views;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using System.Globalization;
using Windows.UI.Xaml.Controls;
using System.Windows.Input;
using MvvmHelpers.Commands;

namespace backlog.ViewModels
{
    public class ImportBacklogViewModel : INotifyPropertyChanged
    {
        private Backlog m_importedBacklog;
        private bool m_signedIn = Settings.IsSignedIn;
        private bool m_isNetworkAvailable;
        private string m_fileName;
        private bool m_isBusy;
        private DateTimeOffset m_dateInput;
        private TimeSpan m_notifTime;
        private bool m_enableNotificationToggle;
        private bool m_showNotificationToggle;
        private bool m_showNotificationOptions;

        public ICommand Import { get; set; }
        public ICommand Cancel { get; set; }

        public DateTime Today = DateTime.Today;

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Backlog> Backlogs;

        public delegate void NavigateToMainPage();
        public NavigateToMainPage NavToMainPageFunc;

        public Backlog ImportedBacklog
        {
            get => m_importedBacklog;
            set
            {
                m_importedBacklog = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImportedBacklog)));
            }
        }

        public string FileName
        {
            get => m_fileName;
            set
            {
                m_fileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
            }
        }

        public bool IsBusy
        {
            get => m_isBusy;
            set
            {
                m_isBusy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }

        public DateTimeOffset DateInput
        {
            get => m_dateInput;
            set
            {
                m_dateInput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DateInput)));
                EnableNotificationToggle = m_dateInput != DateTimeOffset.MinValue;
            }
        }

        public TimeSpan NotifTime
        {
            get => m_notifTime;
            set
            {
                if (m_notifTime != value)
                {
                    m_notifTime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotifTime)));
                }
            }
        }

        public bool EnableNotificationToggle
        {
            get => m_enableNotificationToggle;
            set
            {
                m_enableNotificationToggle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableNotificationToggle)));
            }
        }

        public bool ShowNotificationToggle
        {
            get => m_showNotificationToggle;
            set
            {
                m_showNotificationToggle = value;
                if (m_showNotificationToggle)
                {
                    ShowNotificationOptions = true;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowNotificationToggle)));
            }
        }

        public bool ShowNotificationOptions
        {
            get => m_showNotificationOptions;
            set
            {
                m_showNotificationOptions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowNotificationOptions)));
            }
        }

        public ImportBacklogViewModel()
        {
            m_isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            m_importedBacklog = new Backlog();
            m_importedBacklog.ImageURL = "https://github.com/surya-sk/backlogs/blob/master/backlog/Assets/app-icon.png"; // just a placeholder image

            Import = new AsyncCommand(ImportBacklogAsync);
            Cancel = new Command(CancelImport);
        }

        /// <summary>
        /// Loads the json and creates a Backlog from it
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task LoadBacklogFromFileAsync(string fileName)
        {
            if (fileName != null && fileName != "")
            {
                FileName = fileName;
                IsBusy = true;
                if (m_isNetworkAvailable)
                {
                    await SaveData.GetInstance().ReadDataAsync(m_signedIn);
                    Backlogs = SaveData.GetInstance().GetBacklogs();
                    StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                    StorageFile file = await tempFolder.GetFileAsync(fileName);
                    string json = await FileIO.ReadTextAsync(file);
                    ImportedBacklog = JsonConvert.DeserializeObject<Backlog>(json);
                }
                IsBusy = false;
            }
        }

        /// <summary>
        /// Add imported backlog to user backlogs
        /// </summary>
        /// <returns></returns>
        public async Task ImportBacklogAsync()
        {
            IsBusy = true;
            if (!m_isNetworkAvailable && m_signedIn)
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "No internet",
                    Content = "You need to be connected to the internet for this!",
                    CloseButtonText = "Ok"
                };
                await contentDialog.ShowAsync();
                return;
            }
            if (DateInput != null && DateInput != DateTimeOffset.MinValue)
            {
                string date = DateInput.DateTime.ToString("D", CultureInfo.InvariantCulture);
                if (ShowNotificationOptions)
                {
                    if (NotifTime == TimeSpan.Zero)
                    {
                        ContentDialog contentDialog = new ContentDialog
                        {
                            Title = "Invalid date and time",
                            Content = "Please pick a time!",
                            CloseButtonText = "Ok"
                        };
                        await contentDialog.ShowAsync();
                        return;
                    }
                    DateTimeOffset dateTime = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture).Add(NotifTime);
                    int diff = DateTimeOffset.Compare(dateTime, DateTimeOffset.Now);
                    if (diff < 0)
                    {
                        ContentDialog contentDialog = new ContentDialog
                        {
                            Title = "Invalid time",
                            Content = "The date and time you've chosen are in the past!",
                            CloseButtonText = "Ok"
                        };
                        await contentDialog.ShowAsync();
                        return;
                    }
                }
                else
                {
                    DateTimeOffset dateTime = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture);
                    int diff = DateTime.Compare(DateTime.Today, DateInput.DateTime);
                    if (diff > 0)
                    {
                        ContentDialog contentDialog = new ContentDialog
                        {
                            Title = "Invalid date and time",
                            Content = "The date and time you've chosen are in the past!",
                            CloseButtonText = "Ok"
                        };
                        await contentDialog.ShowAsync();
                        return;
                    }
                }
            }
            ImportedBacklog.id = Guid.NewGuid();
            ImportedBacklog.UserRating = 0;
            ImportedBacklog.Progress = 0;
            ImportedBacklog.CompletedDate = null;
            ImportedBacklog.IsComplete = false;
            ImportedBacklog.CreatedDate = DateTimeOffset.Now.ToString("D", CultureInfo.InvariantCulture);
            ImportedBacklog.TargetDate = DateInput != null ? DateInput.ToString("D", CultureInfo.InvariantCulture) : "None";
            ImportedBacklog.NotifTime = NotifTime;
            Backlogs.Add(ImportedBacklog);
            SaveData.GetInstance().SaveSettings(Backlogs);
            await SaveData.GetInstance().WriteDataAsync(m_signedIn);
            NavToMainPageFunc();
        }

        public void CancelImport()
        {
            NavToMainPageFunc();
        }
    }
}
