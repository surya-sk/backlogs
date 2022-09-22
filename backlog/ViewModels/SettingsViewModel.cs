using backlog.Auth;
using backlog.Logging;
using backlog.Utils;
using backlog.Views;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using MvvmHelpers.Commands;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.Windows.Input;

namespace backlog.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _selectedTheme = Settings.AppTheme;
        private int _selectedTileStyleIndex = Settings.TileStyle == "Peeking" ? 0 : 1;
        private string _tileStylePreviewImage = Settings.TileStyle == "Peeking" ? "ms-appx:///Assets/peeking-tile.png" :
                "ms-appx:///Assets/background-tile.png";
        private bool _showProgress;
        private string _tileContent = Settings.TileContent;

        public delegate void NavigateToMainPage();

        public NavigateToMainPage NavigateToMainPageFunc;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SendLogs { get; }
        public ICommand OpenLogs { get; }
        public ICommand SendFeedback { get; }
        public ICommand SignOut { get; }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                _selectedTheme = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTheme)));
                ChangeAppTheme();
            }
        }

        public int SelectedTileStyleIndex
        {
            get => _selectedTileStyleIndex;
            set
            {
                _selectedTileStyleIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTileStyleIndex)));
                ChangeTileStyle();
            }
        }

        public string SelectedTileContent
        {
            get => _tileContent;
            set
            {
                _tileContent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTileContent)));
                Settings.TileContent = value.ToString();
            }
        }

        public string TileStylePreviewImage
        {
            get => _tileStylePreviewImage;
            set
            {
                _tileStylePreviewImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TileStylePreviewImage)));
            }
        }

        public bool ShowProgress
        {
            get => _showProgress;
            set
            {
                _showProgress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowProgress)));
            }
        }

        public bool AutoplayVideos
        {
            get => Settings.AutoplayVideos;
            set
            {
                Settings.AutoplayVideos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoplayVideos)));
            }
        }

        public string SelectedFeedbackType { get; set; }

        public string FeedbackText { get; set; }
        public SettingsViewModel()
        {
            SendLogs = new AsyncCommand(SendLogsAsync);
            OpenLogs = new AsyncCommand(ShowLogsAsync);
            SendFeedback = new AsyncCommand(SendFeedbackAsync);
            SignOut = new AsyncCommand(SignOutAsync);
        }
        /// <summary>
        /// Change app theme on the fly and save it
        /// </summary>
        private void ChangeAppTheme()
        {
            var _selectedTheme = SelectedTheme;
            if (_selectedTheme != null)
            {
                if (_selectedTheme == "System")
                {
                    _selectedTheme = "Default";
                }
                ThemeHelper.RootTheme = App.GetEnum<ElementTheme>(_selectedTheme);
            }
            Settings.AppTheme = SelectedTheme;
        }

        /// <summary>
        /// Opens email client to send logs
        /// </summary>
        /// <returns></returns>
        private async Task SendLogsAsync()
        {
            ShowProgress = true;
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.To.Add(new EmailRecipient("surya.sk05@outlook.com"));
            emailMessage.Subject = "Logs from Backlogs";
            StringBuilder body = new StringBuilder();
            body.AppendLine("*Enter a brief description of your issue here*");
            body.AppendLine("\n\n\n");
            body.AppendLine("Logs:");
            var logList = await Logger.GetLogsAsync();
            foreach (var log in logList)
            {
                body.AppendLine(log.ToString());
            }
            emailMessage.Body = body.ToString();
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
            ShowProgress = false;
        }

        /// <summary>
        /// Opens a content dialog that shows logs
        /// </summary>
        /// <returns></returns>
        private async Task ShowLogsAsync()
        {
            var logs = await Logger.GetLogsAsync();
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = "Logs",
                Content = new ListView()
                {
                    ItemsSource = logs,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    IsItemClickEnabled = false,
                    SelectionMode = ListViewSelectionMode.None
                },
                CloseButtonText = "Close"
            };
            await contentDialog.ShowAsync();
        }

        /// <summary>
        /// Send user typed feedback
        /// </summary>
        /// <returns></returns>
        private async Task SendFeedbackAsync()
        {
            if (string.IsNullOrEmpty(SelectedFeedbackType) || string.IsNullOrEmpty(FeedbackText))
            {
                await ShowFeedbackInputErrorAsync();
            }
            else
            {
                await SendFeedbackEmailAsync();
            }
        }

        /// <summary>
        /// Show error dialog
        /// </summary>
        /// <returns></returns>
        private async Task ShowFeedbackInputErrorAsync()
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Insufficient data",
                Content = "Please fill in both the fields",
                CloseButtonText = "Ok"
            };
            ContentDialogResult result = await contentDialog.ShowAsync();
        }

        /// <summary>
        /// Open the user's default email client
        /// </summary>
        /// <returns></returns>
        private async Task SendFeedbackEmailAsync()
        {
            ShowProgress = true;
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.Subject = "[Backlogs] " + SelectedFeedbackType;
            emailMessage.Body = FeedbackText;
            emailMessage.To.Add(new EmailRecipient("surya.sk05@outlook.com"));
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
            ShowProgress = false;
        }

        /// <summary>
        /// Sign the user out of MSAL
        /// </summary>
        /// <returns></returns>
        private async Task SignOutAsync()
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Sign out?",
                Content = "You will no longer have access to your backlogs, and new ones will no longer be synced",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };
            ContentDialogResult result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await MSAL.SignOut();
                Settings.IsSignedIn = false;
                NavigateToMainPageFunc();
            }
        }

        /// <summary>
        /// Change tile style
        /// </summary>
        private void ChangeTileStyle()
        {
            TileStylePreviewImage = _selectedTileStyleIndex == 0 ? "ms-appx:///Assets/peeking-tile.png" :
    "ms-appx:///Assets/background-tile.png";
            Settings.TileStyle = _selectedTileStyleIndex == 0 ? "Peeking" : "Background";
        }
    }
}
