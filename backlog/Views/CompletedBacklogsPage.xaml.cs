using backlog.Logging;
using backlog.Models;
using backlog.ViewModels;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompletedBacklogsPage : Page
    {
        private Backlog SelectedBacklog;
        private ICommand CloseBacklogPopup;

        public CompletedBacklogsViewModel ViewModel { get; set; }

        public CompletedBacklogsPage()
        {
            this.InitializeComponent();
            ViewModel = new CompletedBacklogsViewModel(PopupOverlay);

            CloseBacklogPopup = new AsyncCommand(CloseBacklogAsync);

            ViewModel.CloseBacklog = CloseBacklogAsync;
            ViewModel.ClosePopup = Reload;

            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested;
        }

        private void View_BackRequested(object sender, BackRequestedEventArgs e)
        {
            try
            {
                PageStackEntry prevPage = Frame.BackStack.Last();
                try
                {
                    Frame.Navigate(prevPage?.SourcePageType, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                }
                catch
                {
                    Frame.Navigate(prevPage?.SourcePageType);
                }
            }
            catch
            {
                Frame.Navigate(typeof(MainPage));
            }
            e.Handled = true;
        }

        private void Reload()
        {
            Frame.Navigate(typeof(CompletedBacklogsPage));
        }

        /// <summary>
        /// Closes the backlog popup
        /// </summary>
        /// <returns></returns>
        private async Task CloseBacklogAsync()
        {
            try
            {
                ConnectedAnimation connectedAnimation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", destinationGrid);
                PopupOverlay.Hide();
                connectedAnimation.Configuration = new DirectConnectedAnimationConfiguration();
                await MainGrid.TryStartConnectedAnimationAsync(connectedAnimation, SelectedBacklog, "connectedElement");
            }
            catch
            {
                PopupOverlay.Hide();
            }
        }

        private async void MainGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = e.ClickedItem as Backlog;
            SelectedBacklog = selectedBacklog;
            ViewModel.SelectedBacklog = selectedBacklog;
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
            ViewModel.UserRating = selectedBacklog.UserRating;
            ViewModel.UserRating = selectedBacklog.UserRating;

            await PopupOverlay.ShowAsync();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await SearchDialog.ShowAsync();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<string> suggestions = new List<string>();
                var splitText = sender.Text.ToLower().Split(' ');
                ObservableCollection<Backlog> backlogsToSearch = null;
                switch (mainPivot.SelectedIndex)
                {
                    case 0:
                        backlogsToSearch = new ObservableCollection<Backlog>(ViewModel.FinishedBacklogs);
                        break;
                    case 1:
                        backlogsToSearch = new ObservableCollection<Backlog>(ViewModel.FinishedBacklogs);
                        break;
                    case 2:
                        backlogsToSearch = new ObservableCollection<Backlog>(ViewModel.FinishedBacklogs);
                        break;
                    case 3:
                        backlogsToSearch = new ObservableCollection<Backlog>(ViewModel.FinishedBacklogs);
                        break;
                    case 4:
                        backlogsToSearch = new ObservableCollection<Backlog>(ViewModel.FinishedBacklogs);
                        break;
                    case 5:
                        backlogsToSearch = new ObservableCollection<Backlog>(ViewModel.FinishedBacklogs);
                        break;
                }
                foreach (var backlog in backlogsToSearch)
                {
                    var found = splitText.All((key) =>
                    {
                        return backlog.Name.ToLower().Contains(key);
                    });
                    if (found)
                    {
                        suggestions.Add(backlog.Name);
                    }
                }
                if (suggestions.Count == 0)
                {
                    suggestions.Add("No results found");
                }
                sender.ItemsSource = suggestions;
            }
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            mainPivot.SelectedIndex = 0;
            SearchDialog.Hide();
            var selectedBacklog = ViewModel.FinishedBacklogs.FirstOrDefault(b => b.Name == args.ChosenSuggestion.ToString());
            MainGrid.SelectedItem = selectedBacklog;
            MainGrid.ScrollIntoView(selectedBacklog);
        }
    }
}
