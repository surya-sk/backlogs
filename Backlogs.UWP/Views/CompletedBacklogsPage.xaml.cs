using Backlogs.Logging;
using Backlogs.Models;
using Backlogs.Services;
using Backlogs.Utils.UWP;
using Backlogs.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Backlogs.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompletedBacklogsPage : Page
    {
        private Backlog SelectedBacklog;
        public CompletedBacklogsViewModel ViewModel { get; set; }

        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedBacklogs;
        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedFilmBacklogs;
        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedTVBacklogs;
        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedMusicBacklogs;
        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedGameBacklogs;
        public IncrementalLoadingCollection<BacklogSource, Backlog> FinishedBookBacklogs;

        public CompletedBacklogsPage()
        {
            this.InitializeComponent();
            ViewModel = new CompletedBacklogsViewModel(App.Services.GetRequiredService<INavigation>(),
                App.Services.GetService<IUserSettings>());

            FinishedBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(ViewModel.FinishedBacklogs));
            FinishedFilmBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(ViewModel.FinishedFilmBacklogs));
            FinishedGameBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(ViewModel.FinishedGameBacklogs));
            FinishedBookBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(ViewModel.FinishedBookBacklogs));
            FinishedTVBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(ViewModel.FinishedTVBacklogs));
            FinishedMusicBacklogs = new IncrementalLoadingCollection<BacklogSource, Backlog>(new BacklogSource(ViewModel.FinishedMusicBacklogs));

            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            view.BackRequested += View_BackRequested; ;
        }

        private void View_BackRequested(object sender, BackRequestedEventArgs e)
        {
            ViewModel.GoBack();
            e.Handled = true;
        }


        private void MainGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedBacklog = (Backlog)e.ClickedItem;
            PivotItem pivotItem = (PivotItem)mainPivot.SelectedItem;
            // Prepare connected animation based on which section the user is on
            switch (pivotItem.Header.ToString())
            {
                default:
                    BacklogsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "films":
                    FilmsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "tv":
                    TVGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "books":
                    BooksGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "games":
                    GamesGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
                case "albums":
                    AlbumsGrid.PrepareConnectedAnimation("cover", selectedBacklog, "coverImage");
                    break;
            }
            Frame.Navigate(typeof(CompletedBacklogPage), selectedBacklog.id, new SuppressNavigationTransitionInfo());
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await SearchDialog.ShowAsync();
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
            BacklogsGrid.SelectedItem = selectedBacklog;
            BacklogsGrid.ScrollIntoView(selectedBacklog);
        }
    }
}
