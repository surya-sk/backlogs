using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

/// <summary>
/// A class to handle navigation from View Models
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Dictionary<Type, Type> m_viewModelsToViews = new Dictionary<Type, Type>();
    private Frame m_backFrame;

    public void RegisterViewForViewModel(Type viewModel, Type view)
    {
        m_viewModelsToViews[viewModel] = view;
    }

    public void NavigateTo<T>(object args = null, NavigationTransitionInfo navigationTransitionInfo = null)
    {
        ((Frame)Window.Current.Content).Navigate(m_viewModelsToViews[typeof(T)], args, navigationTransitionInfo);
    }

    public void GoBack()
    {
        PageStackEntry _prevPage = m_backFrame.BackStack.Last();
        ((Frame)Window.Current.Content).Navigate(_prevPage?.SourcePageType);
    }

    public void SetFrameForBackNav(Frame frame)
    {
        m_backFrame = frame;
    }
}