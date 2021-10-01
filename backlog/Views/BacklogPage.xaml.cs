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
using backlog.Models;
using backlog.Saving;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BacklogPage : Page
    {
        private Backlog backlog;
        private ObservableCollection<Backlog> backlogs;
        private int backlogIndex;
        private bool edited;
        public BacklogPage()
        {
            this.InitializeComponent();
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            backlogs = SaveData.GetInstance().GetBacklogs();
            edited = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Guid selectedId = (Guid)e.Parameter;
            foreach (Backlog b in backlogs)
            {
                if (selectedId == b.id)
                {
                    backlog = b;
                    backlogIndex = backlogs.IndexOf(b);
                }
            }
            base.OnNavigatedTo(e);
        }

        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = backlog.Name,
                Content = backlog.Description,
                CloseButtonText = "Close"
            };
            await contentDialog.ShowAsync();
        }
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Delete backlog?",
                Content = "Deletion is permanent. This backlog cannot be recovered, and will be gone forever.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };
            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await DeleteConfirmation_Click();
            }
        }

        /// <summary>
        /// Delete a concept after confirmation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task DeleteConfirmation_Click()
        {
            ProgBar.Visibility = Visibility.Visible;
            backlogs.Remove(backlog);
            SaveData.GetInstance().SaveSettings(backlogs);
            await SaveData.GetInstance().WriteDataAsync();
            Frame.Navigate(typeof(MainPage));
        }

        private void NumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            edited = true;
        }

        private async void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            ProgBar.Visibility = Visibility.Visible;
            if(edited)
            {
                backlogs[backlogIndex] = backlog;
                SaveData.GetInstance().SaveSettings(backlogs);
                await SaveData.GetInstance().WriteDataAsync();
            }
            Frame.Navigate(typeof(MainPage));
        }
    }
}
