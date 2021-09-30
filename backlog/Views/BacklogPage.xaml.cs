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
        public BacklogPage()
        {
            this.InitializeComponent();
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            backlogs = SaveData.GetInstance().GetBacklogs();
            ShowBackButton();
        }

        private void ShowBackButton()
        {
            // make the back button visible
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested;
        }

        private void View_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            e.Handled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Guid selectedId = (Guid)e.Parameter;
            foreach (Backlog b in backlogs)
            {
                if (selectedId == b.id)
                {
                    backlog = b;
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
    }
}
