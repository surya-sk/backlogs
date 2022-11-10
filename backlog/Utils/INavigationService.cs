using Windows.UI.Xaml.Media.Animation;

public interface INavigationService
{
    void NavigateTo<T>(object args = null, NavigationTransitionInfo navigationTransitionInfo = null);
}
