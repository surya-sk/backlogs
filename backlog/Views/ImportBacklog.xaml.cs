using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using backlog.Models;
using System.Collections.ObjectModel;
using backlog.Utils;
using System.Net.NetworkInformation;
using backlog.Saving;
using System.Globalization;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportBacklog : Page
    {
        ObservableCollection<Backlog> backlogs { get; set; }
        Backlog importedBacklog;
        bool signedIn;
        bool isNetworkAvailable = false;
        public ImportBacklog()
        {
            this.InitializeComponent();
            signedIn = Settings.IsSignedIn;
            isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            importedBacklog = new Backlog();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string fileName = e.Parameter as string;
            if (fileName != null && fileName != "")
            {
                ProgBar.Visibility = Visibility.Visible;
                if (isNetworkAvailable)
                {
                    await SaveData.GetInstance().ReadDataAsync(signedIn);
                    backlogs = SaveData.GetInstance().GetBacklogs();
                    StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                    StorageFile file = await tempFolder.GetFileAsync(fileName);
                    string json = await FileIO.ReadTextAsync(file);
                    importedBacklog = JsonConvert.DeserializeObject<Backlog>(json);
                    titleText.Text = importedBacklog.Name;
                    directorText.Text = importedBacklog.Director;
                    typeText.Text = importedBacklog.Type;
                    BitmapImage image = new BitmapImage(new Uri(importedBacklog.ImageURL));
                    coverImg.Source = image;
                }
                ProgBar.Visibility = Visibility.Collapsed;
            }
            base.OnNavigatedTo(e);
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ProgBar.Visibility=Visibility.Visible;
            if (datePicker.SelectedDates.Count > 0)
            {
                if (timePicker.Time == TimeSpan.Zero)
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
                string date = datePicker.SelectedDates[0].ToString("D", CultureInfo.InvariantCulture);
                DateTimeOffset dateTime = DateTimeOffset.Parse(date, CultureInfo.InvariantCulture).Add(timePicker.Time);
                int diff = DateTimeOffset.Compare(dateTime, DateTimeOffset.Now);
                if (diff < 0)
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
            else if (timePicker.Time != TimeSpan.Zero)
            {
                if (datePicker.SelectedDates.Count <= 0)
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Invalid date and time",
                        Content = "Please pick a date!",
                        CloseButtonText = "Ok"
                    };
                    await contentDialog.ShowAsync();
                    return;
                }
            }
            importedBacklog.id = Guid.NewGuid();
            importedBacklog.UserRating = 0;
            importedBacklog.Progress = 0;
            importedBacklog.CompletedDate = null;
            importedBacklog.IsComplete = false;
            importedBacklog.CreatedDate = DateTimeOffset.Now.ToString("D", CultureInfo.InvariantCulture);
            importedBacklog.TargetDate = datePicker.SelectedDates.Count > 0 ? datePicker.SelectedDates[0].ToString("D", CultureInfo.InvariantCulture) : "None";
            importedBacklog.NotifTime = timePicker.Time;
            backlogs.Add(importedBacklog);
            SaveData.GetInstance().SaveSettings(backlogs);
            await SaveData.GetInstance().WriteDataAsync(signedIn);
            Frame.Navigate(typeof(MainPage));
         }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
