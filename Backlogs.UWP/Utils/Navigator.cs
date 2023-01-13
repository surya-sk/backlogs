using Backlogs.Services;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Backlogs.ViewModels;
using Backlogs.Views;
using Windows.Foundation.Metadata;

namespace Backlogs.Utils.UWP
{
    public class Navigator : INavigation
    {
        private readonly Dictionary<Type, Type> m_viewModelsToViews = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, NavigationTransitionInfo> m_transitions = new Dictionary<Type, NavigationTransitionInfo>();
        private readonly Dictionary<Type, NavigationTransitionInfo> m_backTransitions = new Dictionary<Type, NavigationTransitionInfo>();


        public void RegisterViewForViewModel(Type viewModel, Type view)
        {
            m_viewModelsToViews[viewModel] = view;
        }

        public void SetAnimations()
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                m_transitions[typeof(BacklogsViewModel)] = new SlideNavigationTransitionInfo()
                { Effect = SlideNavigationTransitionEffect.FromRight };
                m_backTransitions[typeof(BacklogsViewModel)] = new SlideNavigationTransitionInfo()
                { Effect = SlideNavigationTransitionEffect.FromLeft };

                m_transitions[typeof(CompletedBacklogsViewModel)] = new SlideNavigationTransitionInfo()
                { Effect = SlideNavigationTransitionEffect.FromLeft };
                m_backTransitions[typeof(CompletedBacklogsPage)] = new SlideNavigationTransitionInfo()
                { Effect = SlideNavigationTransitionEffect.FromRight };

                m_transitions[typeof(CreateBacklogViewModel)] = new DrillInNavigationTransitionInfo();
                m_backTransitions[typeof(CreateBacklogViewModel)] = new DrillInNavigationTransitionInfo();

                m_transitions[typeof(SettingsPage)] = new SlideNavigationTransitionInfo()
                { Effect = SlideNavigationTransitionEffect.FromBottom };
                m_backTransitions[typeof(SettingsPage)] = new SlideNavigationTransitionInfo()
                { Effect = SlideNavigationTransitionEffect.FromBottom };
            }
        }

        public void GoBack<T>()
        {
            try
            {
                GoBack(m_backTransitions[typeof(T)]);
            }
            catch
            {
                GoBack(null);
            }
        }

        public void NavigateTo<T>(object args = null)
        {
            try
            {
               ((Frame)Window.Current.Content).Navigate(m_viewModelsToViews[typeof(T)], args, m_transitions[typeof(T)]);
            }
            catch
            {
                ((Frame)Window.Current.Content).Navigate(m_viewModelsToViews[typeof(T)], args, null);
            }
        }

        public void GoBack(NavigationTransitionInfo navigationTransitionInfo)
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
