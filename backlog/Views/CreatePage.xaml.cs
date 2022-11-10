using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using System.Linq;
using backlog.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreatePage : Page
    {
        public CreateBacklogViewModel ViewModel { get; set; } 
        int typeIndex = 0;

        public CreatePage()
        {
            this.InitializeComponent();
            var _navService = App.GetNavigationService();
            ViewModel = new CreateBacklogViewModel(resultsDialog, _navService);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter != null)
            {
                typeIndex = (int)e.Parameter;
                if(typeIndex > 0)
                {
                    ViewModel.SelectedIndex = typeIndex-1;
                }
            }
            await ViewModel.SyncBacklogs();
            base.OnNavigatedTo(e);
        }

        private async void NameInput_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                await ViewModel.TrySearchBacklogAsync();
            }
        }
    }
}
