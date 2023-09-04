using Backlogs.Services;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
//using Windows.UI.Xaml.Media.Animation;
using Backlogs.ViewModels;
//using Backlogs.Views;
using Windows.Foundation.Metadata;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;

namespace Backlogs.Utils.Uno
{
    public class Navigator : INavigation
    {
        private readonly Dictionary<Type, Type> m_viewModelsToViews = new Dictionary<Type, Type>();


        public void RegisterViewForViewModel(Type viewModel, Type view)
        {
            m_viewModelsToViews[viewModel] = view;
        }

        public void SetAnimations()
        {
            
        }

        public void GoBack<T>()
        {
                GoBack(null);
      
        }

        public void NavigateTo<T>(object? args = null)
        {

                ((Frame)Window.Current.Content).Navigate(m_viewModelsToViews[typeof(T)], args, null);
        }

        public void GoBack(NavigationTransitionInfo? navigationTransitionInfo)
        {
            try
            {
                try
                {
                    ((Frame)Window.Current.Content).GoBack(navigationTransitionInfo);
                }
                catch
                {
                    ((Frame)Window.Current.Content).GoBack();
                }
            }
            catch
            {
                NavigateTo<MainViewModel>();
            }
        }
    }
}
