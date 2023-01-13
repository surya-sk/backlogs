using System;

namespace Backlogs.Services
{
    /// <summary>
    /// Manages navigation between pages
    /// </summary>
    public interface INavigation
    {
        /// <summary>
        /// Registers views for view models to map them when navigating 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="view"></param>
        void RegisterViewForViewModel(Type viewModel, Type view);

        /// <summary>
        /// Navigates to the view associated with the ViewModel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        void NavigateTo<T>(object args = null);

        /// <summary>
        /// Navigates to the previous page
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void GoBack<T>();

        /// <summary>
        /// Initializes page transition animations
        /// </summary>
        void SetAnimations();
    }
}
