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
            ViewModel = new ImportBacklogViewModel(App.Services.GetRequiredService<INavigation>(), 
                App.Services.GetRequiredService<IDialogHandler>(),
                App.Services.GetRequiredService<IFileHandler>(), App.Services.GetService<IUserSettings>());
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string fileName = e.Parameter as string;
            base.OnNavigatedTo(e);
            await ViewModel.LoadBacklogFromFileAsync(fileName);
        }
    }
}
