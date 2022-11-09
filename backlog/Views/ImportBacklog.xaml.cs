using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using backlog.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportBacklog : Page
    {
        public ImportBacklogViewModel ViewModel { get; set; } = new ImportBacklogViewModel();
        public ImportBacklog()
        {
            this.InitializeComponent();
            ViewModel.NavToMainPageFunc = NavigateToMainPage;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string fileName = e.Parameter as string;
            await ViewModel.LoadBacklogFromFileAsync(fileName);
            base.OnNavigatedTo(e);
        }

        private void NavigateToMainPage()
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
