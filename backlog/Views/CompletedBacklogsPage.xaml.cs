using backlog.Logging;
using backlog.Models;
using backlog.Saving;
using backlog.Utils;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompletedBacklogsPage : Page
    {
        private ObservableCollection<Backlog> FinishedBacklogs;
        private ObservableCollection<Backlog> Backlogs;
        private Backlog SelectedBacklog;

        public CompletedBacklogsPage()
        {
            this.InitializeComponent();
            Task.Run(async () => { await SaveData.GetInstance().ReadDataAsync(); }).Wait();
            Backlogs = SaveData.GetInstance().GetBacklogs();
            FinishedBacklogs = new ObservableCollection<Backlog>(Backlogs.Where(b => b.IsComplete));
            if(FinishedBacklogs.Count < 1)
            {
                EmptyText.Visibility = Visibility.Visible;
                MainGrid.Visibility = Visibility.Collapsed; 
            }
            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested;
        }

        private void View_BackRequested(object sender, BackRequestedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            catch
            {
                Frame.Navigate(typeof(MainPage));
            }
            e.Handled = true;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ProgRing.IsActive = true;
            foreach(var backlog in Backlogs)
            {
                if(backlog.id == SelectedBacklog.id)
                {
                    backlog.UserRating = PopupRating.Value;
                }
            }
            foreach (var backlog in FinishedBacklogs)
            {
                if (backlog.id == SelectedBacklog.id)
                {
                    backlog.UserRating = PopupRating.Value;
                }
            }
            SaveData.GetInstance().SaveSettings(Backlogs);
            await SaveData.GetInstance().WriteDataAsync(Settings.IsSignedIn);
            ProgRing.IsActive = false;
            CloseButton_Click(sender, e);
        }

        private async void IncompleteButton_Click(object sender, RoutedEventArgs e)
        {
            ProgRing.IsActive = true;
            foreach (var backlog in Backlogs)
            {
                if (backlog.id == SelectedBacklog.id)
                {
                    backlog.IsComplete = false;
                    backlog.CompletedDate = null;
                }
            }
            SaveData.GetInstance().SaveSettings(Backlogs);
            await SaveData.GetInstance().WriteDataAsync(Settings.IsSignedIn);
            PopupOverlay.Hide();
            Frame.Navigate(typeof(CompletedBacklogsPage));
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectedAnimation connectedAnimation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", destinationGrid);
            PopupOverlay.Hide();
            try
            {
                connectedAnimation.Configuration = new DirectConnectedAnimationConfiguration();
                await MainGrid.TryStartConnectedAnimationAsync(connectedAnimation, SelectedBacklog, "connectedElement");
            }
            catch(Exception ex)
            {
                await Logger.Error("Error with connected animation", ex);
            }
        }

        private async void MainGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = e.ClickedItem as Backlog;
            SelectedBacklog = selectedBacklog;
            try
            {
                ConnectedAnimation connectedAnimation = MainGrid.PrepareConnectedAnimation("forwardAnimation", SelectedBacklog, "connectedElement");
                connectedAnimation.Configuration = new DirectConnectedAnimationConfiguration();
                connectedAnimation.TryStart(destinationGrid);
            }
            catch (Exception ex)
            {
                await Logger.Error("Error with connected animation", ex);
                // ; )
            }

            PopupImage.Source = new BitmapImage(new Uri(selectedBacklog.ImageURL));
            PopupTitle.Text = selectedBacklog.Name;
            PopupDirector.Text = selectedBacklog.Director;
            PopupRating.Value = selectedBacklog.UserRating;
            PopupSlider.Value = selectedBacklog.UserRating;

            await PopupOverlay.ShowAsync();
        }

        private void PopupSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            PopupRating.Value = e.NewValue;
        }
    }
}
