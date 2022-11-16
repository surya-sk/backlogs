using Backlogs.Auth;
using Backlogs.Utils;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using MvvmHelpers.Commands;
using System.ComponentModel;
using System.Windows.Input;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Logger = Backlogs.Logging.Logger;
using Windows.UI.Core;
using Backlogs.Services;
using Windows.UI.Xaml;

namespace Backlogs.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {

        private string m_selectedTheme = Settings.AppTheme;
        private int m_selectedTileStyleIndex = Settings.TileStyle == "Peeking" ? 0 : 1;
        private string m_tileStylePreviewImage = Settings.TileStyle == "Peeking" ? "ms-appx:///Assets/peeking-tile.png" :
                "ms-appx:///Assets/background-tile.png";
        private bool m_showProgress;
        private string m_tileContent = Settings.TileContent;
        private BitmapImage m_accountPic;
        private readonly INavigationService m_navigationService;
        private readonly IDialogHandler m_dialogHander;


        public string MIT { get; } = "Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: \n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. \n\nTHE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";
        public string GPL { get; } = "This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.\n\nThis program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. \n\nYou should have received a copy of the GNU General Public License along with this program. If not, see https://www.gnu.org/licenses/";
        public string Changelog { get; } = "\u2022 Film, TV and game backlogs now have a button to play the trailer.\n" +
            "\u2022 There is now a button that opens web results for the backlog.\n" +
            "\u2022 Today's date is now the minimum date when picking target date.\n" +
            "\u2022 The homepage now shows upcoming backlogs.\n" +
            "\u2022 The app can now show upcoming backlogs in the live tile.\n";
        public string ChangelogTitle { get; } = "New this version - 30 July, 2022";
        public string Version { get; } = Settings.Version;

        public bool SignedIn { get; } = Settings.IsSignedIn;

        public bool ShowSignInPrompt { get; } = !Settings.IsSignedIn;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SendLogs { get; }
        public ICommand OpenLogs { get; }
        public ICommand SendFeedback { get; }
        public ICommand SignOut { get; }

        public string SelectedTheme
        {
            get => m_selectedTheme;
            set
            {
                if(m_selectedTheme != value)
                {
                    m_selectedTheme = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTheme)));
                    ChangeAppTheme();
                }
            }
        }

        public int SelectedTileStyleIndex
        {
            get => m_selectedTileStyleIndex;
            set
            {
                m_selectedTileStyleIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTileStyleIndex)));
                ChangeTileStyle();
            }
        }

        public string SelectedTileContent
        {
            get => m_tileContent;
            set
            {
                if(m_tileContent != value)
                {
                    m_tileContent = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTileContent)));
                    Settings.TileContent = value.ToString();
                }
            }
        }

        public string TileStylePreviewImage
        {
            get => m_tileStylePreviewImage;
            set
            {
                m_tileStylePreviewImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TileStylePreviewImage)));
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

        public bool AutoplayVideos
        {
            get => Settings.AutoplayVideos;
            set
            {
                Settings.AutoplayVideos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoplayVideos)));
            }
        }

        public BitmapImage AccountPic
        {
            get => m_accountPic;
            set
            {
                m_accountPic = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountPic)));
            }
        }

        public string UserGreeting { get; } = $"Hey there, {Settings.UserName}! You are all synced.";

        public string SelectedFeedbackType { get; set; }

        public string FeedbackText { get; set; }
        public SettingsViewModel(INavigationService navigationService, IDialogHandler dialogHandler)
        {
            SendLogs = new AsyncCommand(SendLogsAsync);
            OpenLogs = new AsyncCommand(ShowLogsAsync);
            SendFeedback = new AsyncCommand(SendFeedbackAsync);
            SignOut = new AsyncCommand(SignOutAsync);
            m_navigationService = navigationService;
            m_dialogHander = dialogHandler;
        }

        /// <summary>
        /// Show the user photo
        /// </summary>
        /// <returns></returns>
        public async Task SetUserPhotoAsync()
        {
            var cacheFolder = ApplicationData.Current.LocalCacheFolder;
            try
            {
                var accountPicFile = await cacheFolder.GetFileAsync("profile.png");
                using (IRandomAccessStream stream = await accountPicFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage image = new BitmapImage();
                    stream.Seek(0);
                    await image.SetSourceAsync(stream);
                    AccountPic = image;
                }
            }
            catch
            {
                // No image set
            }
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
            await m_dialogHander.ShowLogsDialogAsyncAsync(logs);
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
            await m_dialogHander.ShowErrorDialogAsync("Insufficient data", "Please fill in both the fields", "Ok");
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
            if (await m_dialogHander.ShowSignOutDialogAsync())
            {
                await MSAL.SignOut();
                Settings.IsSignedIn = false;
                m_navigationService.NavigateTo<MainViewModel>();
            }
        }

        /// <summary>
        /// Change tile style
        /// </summary>
        private void ChangeTileStyle()
        {
            TileStylePreviewImage = m_selectedTileStyleIndex == 0 ? "ms-appx:///Assets/peeking-tile.png" :
    "ms-appx:///Assets/background-tile.png";
            Settings.TileStyle = m_selectedTileStyleIndex == 0 ? "Peeking" : "Background";
        }


        /// <summary>
        /// Navigates to previous frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GoBack(object sender, BackRequestedEventArgs e)
        {
            m_navigationService.GoBack();
            e.Handled = true;
        }
    }
}
