using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Backlogs.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Backlogs.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Backlogs.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportBacklog : Page
    {
        public ImportBacklogViewModel ViewModel { get; set; } 
        public ImportBacklog()
        {
            this.InitializeComponent();
            ViewModel = new ImportBacklogViewModel(App.GetNavigationService(), App.Services.GetRequiredService<IDialogHandler>());
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string fileName = e.Parameter as string;
            await ViewModel.LoadBacklogFromFileAsync(fileName);
            base.OnNavigatedTo(e);
        }
    }
}
