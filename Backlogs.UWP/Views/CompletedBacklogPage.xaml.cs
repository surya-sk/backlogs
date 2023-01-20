using Backlogs.Services;
using Backlogs.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Backlogs.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompletedBacklogPage : Page
    {
        private CompletedBacklogViewModel ViewModel { get; set; }
        public CompletedBacklogPage()
        {
            this.InitializeComponent();

            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Guid selectedId = (Guid)e.Parameter;
            ViewModel = new CompletedBacklogViewModel(selectedId, App.Services.GetRequiredService<INavigation>(),
                App.Services.GetRequiredService<IUserSettings>(), App.Services.GetRequiredService<IShareDialogService>());
            base.OnNavigatedTo(e);
            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("cover");
            imageAnimation?.TryStart(img);
        }
    }
}
