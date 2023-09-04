﻿using Backlogs.Constants;
//using Backlogs.Controls;
using Backlogs.Models;
using Backlogs.Services;
using Microsoft.Extensions.DependencyInjection;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Backlogs.Uno.Controls;

namespace Backlogs.Utils.Uno
{
    public class DialogHandler : IDialogHandler
    {
       // private ContentDialog? m_trailerDialog;
        //private WebView? m_trailerWebView;

        public void CloseTrailerDialog()
        {
            //m_trailerWebView.Navigate(new Uri("about:blank"));
            //m_trailerDialog?.Hide();
        }

        public async Task OpenTrailerDialogAsync(string video)
        {
            //m_trailerWebView = new WebView();
            //Grid grid = new Grid();
            //grid.Width = 600;
            //grid.Height = 400;
            //grid.Children.Add(m_trailerWebView);
            //m_trailerDialog = new ContentDialog()
            //{
            //    CloseButtonText = "Close",
            //    CloseButtonCommand = new Command(CloseTrailerDialog),
            //    Content = grid
            //};
            //await m_trailerDialog.ShowAsync();
            //try
            //{
            //    m_trailerDialog.CornerRadius = new CornerRadius(0); // Without this, for some fucking reason, buttons inside the WebView do not work
            //}
            //catch { }
            //string width = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile" ? "600" : "500";
            //string height = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile" ? "100%" : "400";
            //try
            //{
            //    m_trailerWebView.NavigateToString($"<iframe width=\"{width}\" height=\"{height}\" src=\"https://www.youtube.com/embed/{video}?autoplay={App.Services.GetRequiredService<IUserSettings>().Get<bool>(SettingsConstants.AutoplayVideos)}\" title=\"YouTube video player\"  allow=\"accelerometer; autoplay; encrypted-media; gyroscope;\"></iframe>");
            //}
            //catch(Exception ex)
            //{
            //    await ShowErrorDialogAsync("CANNOT PLAY TRAILER", ex.Message, "Close");
            //}
            throw new NotImplementedException();
        }

        public async Task ReadMoreDialogAsync(Backlog backlog)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = backlog.Name.ToUpper(),
                Content = backlog.Description,
                CloseButtonText = "Close"
            };
            await contentDialog.ShowAsync();
        }

        public async Task<bool> ShowCrashLogDialogAsync(string log)
        {
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = "OOOPS...CRASH!",
                Content = $"It seems the application crashed the last time, with the following error: {log}",
                PrimaryButtonText = "Send to developer",
                CloseButtonText = "Close"
            };
            var resullt = await contentDialog.ShowAsync();
            return resullt == ContentDialogResult.Primary;
        }

        public async Task<bool> ShowDeleteConfirmationDialogAsync()
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Delete backlog?".ToUpper(),
                Content = "Deletion is permanent. This backlog cannot be recovered, and will be gone forever.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };
            ContentDialogResult result = await deleteDialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        public async Task ShowErrorDialogAsync(string title, string message, string closeText)
        {
            ContentDialog content = new ContentDialog()
            {
                Title = title.ToUpper(),
                Content = message,
                CloseButtonText = closeText
            };
            await content.ShowAsync();
        }

        public async Task ShowLogsDialogAsyncAsync(List<Log> logs)
        {
            //LogsDialog logsDialog = new LogsDialog(logs);
            //await logsDialog.ShowAsync();
            throw new NotImplementedException();
        }

        public async Task<int> ShowRandomBacklogDialogAsync(Backlog backlog)
        {
            ContentDialog contentDialog = new ContentDialog()
            {
                Title = "Your Pick".ToUpper(),
                Content = $"Your current pick is {backlog.Name} by {backlog.Director}",
                CloseButtonText = "Ok",
                PrimaryButtonText = "Go again",
                SecondaryButtonText = "Open"
            };
            var result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary) return 1;
            else if (result == ContentDialogResult.Secondary) return 2;
            else return 0;
        }

        public async Task<SearchResult?> ShowSearchResultsDialogAsync(string name, ObservableCollection<SearchResult> searchResults)
        {
            SearchResultsDialog searchResultsDialog = new SearchResultsDialog(name, searchResults);
            var result = await searchResultsDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                return (SearchResult)searchResultsDialog.Content;
            }
            return null;
        }

        public async Task<bool> ShowSignOutDialogAsync()
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Sign out?".ToUpper(),
                Content = "You will no longer have access to your backlogs, and new ones will no longer be synced",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };
            var result = await contentDialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
    }
}
