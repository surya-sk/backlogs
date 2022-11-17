using Backlogs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace Backlogs.Utils.UWP
{
    public class Navigator : INavigation
    {
        private readonly Dictionary<Type, Type> m_viewModelsToViews = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, NavigationTransitionInfo> m_transitions = new Dictionary<Type, NavigationTransitionInfo>()
        {

        };

        public void RegisterViewForViewModel(Type viewModel, Type view)
        {
            m_viewModelsToViews[viewModel] = view;
        }

        public void GoBack<T>()
        {
            ((Frame)Window.Current.Content).GoBack(m_transitions[typeof(T)]);
        }

        public void NavigateTo<T>(object args = null)
        {
            ((Frame)Window.Current.Content).Navigate(m_viewModelsToViews[typeof(T)], args, m_transitions[typeof(T)]);
        }

        public void GoBack()
        {
            ((Frame)Window.Current.Content).GoBack();
        }
    }
}
