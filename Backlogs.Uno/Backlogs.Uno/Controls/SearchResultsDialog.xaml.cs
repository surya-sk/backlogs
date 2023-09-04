using Backlogs.Models;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Backlogs.Uno.Controls
{
    public sealed partial class SearchResultsDialog : ContentDialog
    {
        SearchResult? selectedResult;
        public SearchResultsDialog(string name, ObservableCollection<SearchResult> searchResults)
        {
            this.InitializeComponent();
            this.IsPrimaryButtonEnabled = false;
            resultTitle.Text = $"results for {name}".ToUpper();
            resultsList.ItemsSource = searchResults;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            selectedResult = (SearchResult)e.ClickedItem;
            if(selectedResult != null)
            {
                this.IsPrimaryButtonEnabled = true;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Content = selectedResult;
        }
    }
}
