using Backlogs.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Backlogs.Controls
{
    public sealed partial class SearchResultsDialog : ContentDialog
    {
        SearchResult selectedResult;
        public SearchResultsDialog(string name, ObservableCollection<SearchResult> searchResults)
        {
            this.InitializeComponent();
            this.IsPrimaryButtonEnabled = false;
            resultTitle.Text = $"Showing results for {name}";
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
