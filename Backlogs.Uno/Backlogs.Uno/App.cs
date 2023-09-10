using Backlogs.Uno.Views;
using Backlogs.Utils.Uno;
using Backlogs.Services;
using Backlogs.ViewModels;
using Backlogs.Constants;
using Backlogs.Utils;

namespace Backlogs.Uno
{
    public class App : Application
    {
        private IServiceProvider? m_serviceProvider;
        private IUserSettings? m_userSettings;
        private INavigation? m_navigationService;
        private IFileHandler? m_fileHander;
        public static Window? _window;

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            #if NET6_0_OR_GREATER && WINDOWS && !HAS_UNO
		            _window = new Window();
            #else
                        _window = Microsoft.UI.Xaml.Window.Current;

           
                        m_serviceProvider = ConfigureServices();
            #endif

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (_window.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Place the frame in the current Window
                _window.Content = rootFrame;

                rootFrame.NavigationFailed += OnNavigationFailed;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter

                

                m_serviceProvider = ConfigureServices();
                m_userSettings = Services.GetRequiredService<IUserSettings>();
                //m_userSettings.UserSettingsChanged += M_userSettings_UserSettingsChanged;
                //rootFrame.ActualThemeChanged += RootFrame_ActualThemeChanged;
                m_fileHander = Services.GetRequiredService<IFileHandler>();
                m_navigationService = Services.GetRequiredService<INavigation>();
                m_navigationService.RegisterViewForViewModel(typeof(MainViewModel), typeof(MainPage));
                //m_navigationService.RegisterViewForViewModel(typeof(BacklogsViewModel), typeof(BacklogsPage));
                //m_navigationService.RegisterViewForViewModel(typeof(BacklogViewModel), typeof(BacklogPage));
                //m_navigationService.RegisterViewForViewModel(typeof(CompletedBacklogsViewModel), typeof(CompletedBacklogsPage));
                //m_navigationService.RegisterViewForViewModel(typeof(CompletedBacklogViewModel), typeof(CompletedBacklogPage));
                m_navigationService.RegisterViewForViewModel(typeof(CreateBacklogViewModel), typeof(CreatePage));
                //m_navigationService.RegisterViewForViewModel(typeof(ImportBacklogViewModel), typeof(ImportBacklog));
                m_navigationService.RegisterViewForViewModel(typeof(SettingsViewModel), typeof(SettingsPage));

                BacklogsManager.GetInstance().InitBacklogsManager(m_fileHander);
                Task.Run(async () => { await BacklogsManager.GetInstance().ReadDataAsync(); }).Wait();
                BacklogsManager.GetInstance().ResetHelperBacklogs();

                rootFrame.Navigate(typeof(MainPage), args.Arguments);


            }

            // Ensure the current window is active
            _window.Activate();

            
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        private static IServiceProvider ConfigureServices()
        {
            var provider = new ServiceCollection()
                .AddSingleton<IUserSettings, Settings>()
                .AddSingleton<IMsal, MSAL>()
                .AddSingleton<IDialogHandler, DialogHandler>()
                .AddSingleton<IEmailService, EmailHandler>()
                .AddSingleton<IFileHandler, Backlogs.Utils.Uno.FileIO>()
                .AddSingleton<IFilePicker, FilePickerService>()
                .AddSingleton<IShareDialogService, ShareDialogService>()
                .AddSingleton<IToastNotificationService, ToastNotificationService>()
                .AddSingleton<INavigation, Navigator>()
                .AddSingleton<ISystemLauncher, SystemLauncher>()
                .BuildServiceProvider();
            return provider;
        }

        public static IServiceProvider Services
        {
            get
            {
                IServiceProvider? serviceProvider = ((App)Current).m_serviceProvider;
                if (serviceProvider == null)
                {
                    throw new InvalidOperationException("Service is not initialized");
                }
                return serviceProvider;
            }
        }
    }
}