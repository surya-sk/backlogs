using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using backlog.Utils;
using System.Linq;
using Windows.UI.Core;
using Windows.System.Profile;
using System.Windows.Input;
using MvvmHelpers.Commands;
using backlog.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace backlog.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BacklogPage : Page
    {
        public BacklogViewModel ViewModel { get; set; }


        public BacklogPage()
        {
            this.InitializeComponent();

            var view = SystemNavigationManager.GetForCurrentView();
            view.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Guid selectedId = (Guid)e.Parameter;
            ViewModel = new BacklogViewModel(selectedId, ratingDialog:RatingDialog, trailerDialog:trailerDialog, webView:webView, App.GetNavigationService());
            base.OnNavigatedTo(e);
            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("cover");
            imageAnimation?.TryStart(img);
        }
    }
}
