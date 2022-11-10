using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

/// <summary>
/// A class to handle navigation from View Models
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Dictionary<Type, Type> m_viewModelsToViews = new Dictionary<Type, Type>();

    public void RegisterViewForViewModel(Type viewModel, Type view)
    {
        m_viewModelsToViews[viewModel] = view;
    }

    public void NavigateTo<T>()
    {
        ((Frame)Window.Current.Content).Navigate(m_viewModelsToViews[typeof(T)]);
    }
}