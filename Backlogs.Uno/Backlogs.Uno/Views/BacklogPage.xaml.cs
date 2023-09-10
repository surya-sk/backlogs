using Backlogs.Services;
using Backlogs.ViewModels;

namespace Backlogs.Uno.Views
{
    public sealed partial class BacklogPage : Page
    {
        public BacklogViewModel? ViewModel { get; set; }

        public BacklogPage()
        {
            this.InitializeComponent();
            this.Loading += BacklogPage_Loading;

        }
        private async void BacklogPage_Loading(FrameworkElement sender, object args)
        {
            await ViewModel!.GetDetailsAsync();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Guid selectedId = (Guid)e.Parameter;
            ViewModel = new BacklogViewModel(selectedId, App.Services.GetRequiredService<INavigation>(),
                App.Services.GetRequiredService<IDialogHandler>(), App.Services.GetRequiredService<IToastNotificationService>(),
                App.Services.GetRequiredService<IShareDialogService>(), App.Services.GetRequiredService<IUserSettings>(),
                App.Services.GetRequiredService<IFileHandler>(), App.Services.GetRequiredService<ISystemLauncher>());
            base.OnNavigatedTo(e);
        }

    }

}
