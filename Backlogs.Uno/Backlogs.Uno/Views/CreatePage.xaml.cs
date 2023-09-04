using Backlogs.Services;
using Backlogs.ViewModels;

namespace Backlogs.Uno.Views
{
    public sealed partial class CreatePage : Page
    {
        public CreateBacklogViewModel viewModel { get; set; }

        public CreatePage()
        {
            this.InitializeComponent();
            viewModel=new CreateBacklogViewModel(App.Services.GetRequiredService<INavigation>(),
                App.Services.GetRequiredService<IDialogHandler>(), 
                App.Services.GetRequiredService<IToastNotificationService>(), 
                App.Services.GetRequiredService<IUserSettings>(), 
                App.Services.GetRequiredService<IFileHandler>());


        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await viewModel.SyncBacklogs();
        }

    }


}