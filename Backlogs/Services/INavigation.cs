using System;

namespace Backlogs.Services
{
    public interface INavigation
    {
        void RegisterViewForViewModel(Type viewModel, Type view);
        void NavigateTo<T>(object args = null);
        void GoBack<T>();
        void SetAnimations();
    }
}
