using Microsoft.UI.Xaml.Controls;

namespace Backlogs.Uno.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void ShowPrompt_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog promptDialog = new ContentDialog
            {
                Title = "Prompt",
                Content = "Do you want to continue?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            ContentDialogResult result = await promptDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // User clicked the "Yes" button
            }
            else if (result == ContentDialogResult.Secondary)
            {
                // User clicked the "No" button or closed the dialog
            }

        }
    }
}