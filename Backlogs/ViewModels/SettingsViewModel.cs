using System;
using System.Text;
using System.Threading.Tasks;
using MvvmHelpers.Commands;
using System.ComponentModel;
using System.Windows.Input;
using Backlogs.Services;
using Backlogs.Constants;
using System.Diagnostics;

namespace Backlogs.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private bool m_showProgress;
        private string m_accountPic = "https://github.com/surya-sk/backlogs/blob/master/backlog/Assets/app-icon.png";
        private readonly INavigation m_navigationService;
        private readonly IDialogHandler m_dialogHander;
        private readonly IFileHandler m_fileHandler;
        private readonly IEmailService m_emailService;
        private IUserSettings m_settings;
        private IMsal m_msal;
        private string m_selectedTheme;
        private int m_selectedTileStyleIndex;
        private string m_tileContent;
        private string m_tileStylePreviewImage;

        public string MIT { get; } = "Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: \n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. \n\nTHE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";
        public string GPL { get; } = "This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.\n\nThis program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. \n\nYou should have received a copy of the GNU General Public License along with this program. If not, see https://www.gnu.org/licenses/";
        public string Changelog { get; } = "\u2022 Completed backlogs now open in their own page\n" +
            "\u2022 The UI for rating a backlog and marking it as complete is now improved.\n" +
            "\u2022 Loading backlogs should be much quicker now thanks to incremental loading.\n" +
            "\u2022 From this version onwards, the app only supports Windows 10 1709 and up.\n" +
            "\u2022 Removed page transition animation for this release.\n";
        public string ChangelogTitle { get; } = "New this version - 07 December, 2022";
        public string Version { get => m_settings.Get<string>(SettingsConstants.Version); } 

        public bool ShowSignInPrompt { get; } 

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SendLogs { get; }
        public ICommand OpenLogs { get; }
        public ICommand SendFeedback { get; }
        public ICommand SignOut { get; }

        #region Properties
        public string SelectedTheme
        {
            get
            {
                m_selectedTheme = m_settings.Get<string>(SettingsConstants.AppTheme);
                return m_selectedTheme;
            }
            set
            {
                if(m_selectedTheme != value)
                {
                    m_selectedTheme = value;
                    m_settings.Set(SettingsConstants.AppTheme, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTheme)));
                }
            }
        }

        public int SelectedTileStyleIndex
        {
            get
            {
                m_selectedTileStyleIndex = m_settings.Get<string>(SettingsConstants.TileStyle) == "Peeking" ? 0 : 1;
                return m_selectedTileStyleIndex;
            }
            set
            {
                m_selectedTileStyleIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTileStyleIndex)));
                ChangeTileStyle();
            }
        }

        public string SelectedTileContent
        {
            get
            {
                m_tileContent = m_settings.Get<string>(SettingsConstants.TileContent);
                return m_tileContent;
            }
            set
            {
                if(m_tileContent != value)
                {
                    m_tileContent = value;
                    m_settings.Set<string>(SettingsConstants.TileContent, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTileContent)));
                }
            }
        }

        public string TileStylePreviewImage
        {
            get
            {
                m_tileStylePreviewImage = m_settings.Get<string>(SettingsConstants.TileStyle) == "Peeking" ?
                    "ms-appx:///Assets/peeking-tile.png" : "ms-appx:///Assets/background-tile.png";
                return m_tileStylePreviewImage;
            }
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
            get => m_settings.Get<bool>(SettingsConstants.AutoplayVideos);
            set
            {
                m_settings.Set(SettingsConstants.AutoplayVideos, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoplayVideos)));
            }
        }

        public string AccountPic
        {
            get => m_accountPic;
            set
            {
                m_accountPic = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountPic)));
            }
        }

        public bool SignedIn
        {
            get => m_settings.Get<bool>(SettingsConstants.IsSignedIn);
        }
        #endregion

        public string UserGreeting { get => $"Hey there, {m_settings.Get<string>(SettingsConstants.UserName)}! You are all synced."; }

        public string SelectedFeedbackType { get; set; }

        public string FeedbackText { get; set; }
        public SettingsViewModel(INavigation navigationService, IDialogHandler dialogHandler, IFileHandler fileHandler,
            IEmailService emailService, IUserSettings settings, IMsal msal)
        {
            SendLogs = new AsyncCommand(SendLogsAsync);
            OpenLogs = new AsyncCommand(ShowLogsAsync);
            SendFeedback = new AsyncCommand(SendFeedbackAsync);
            SignOut = new AsyncCommand(SignOutAsync);
            m_navigationService = navigationService;
            m_dialogHander = dialogHandler;
            m_fileHandler = fileHandler;
            m_emailService = emailService;
            m_settings = settings;
            m_msal = msal;
        }

        /// <summary>
        /// Show the user photo
        /// </summary>
        /// <returns></returns>
        public async Task SetUserPhotoAsync()
        {
            if (!SignedIn) return;
            try
            {
                AccountPic = await m_fileHandler.ReadImageAsync("profile.png");
            }
            catch
            {
                // No image set
            }
        }

        /// <summary>
        /// Opens email client to send logs
        /// </summary>
        /// <returns></returns>
        private async Task SendLogsAsync()
        {
            ShowProgress = true;
            var subject = "Logs from Backlogs";
            StringBuilder body = new StringBuilder();
            body.AppendLine("*Enter a brief description of your issue here*");
            body.AppendLine("\n\n\n");
            body.AppendLine("Logs:");
            var logList = await m_fileHandler.ReadLogsAync();
            foreach (var log in logList)
            {
                body.AppendLine(log.ToString());
            }
            await m_emailService.SendEmailAsync(subject, body.ToString());
            ShowProgress = false;
        }

        /// <summary>
        /// Opens a content dialog that shows logs
        /// </summary>
        /// <returns></returns>
        private async Task ShowLogsAsync()
        {
            var logs = await m_fileHandler.ReadLogsAync();
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
            var subject = "[Backlogs] " + SelectedFeedbackType;
            var body = FeedbackText;
            await m_emailService.SendEmailAsync(subject, body);
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
                await m_msal.SignOut();
                m_settings.Set(SettingsConstants.IsSignedIn, false);
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
            m_settings.Set(SettingsConstants.TileStyle, m_selectedTileStyleIndex == 0 ? "Peeking" : "Background");
        }


        /// <summary>
        /// Navigates to previous frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GoBack()
        {
            m_navigationService.GoBack<SettingsViewModel>();
        }
    }
}
