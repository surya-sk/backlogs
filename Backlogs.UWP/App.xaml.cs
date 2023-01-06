using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Backlogs.Views;
using System.Reflection;
using Windows.Storage;
using System.Threading.Tasks;
using Backlogs.ViewModels;
using Backlogs.Services;
using Microsoft.Extensions.DependencyInjection;
using Backlogs.Utils.UWP;
using Settings = Backlogs.Utils.UWP.Settings;
using Backlogs.Constants;
using FileIO = Windows.Storage.FileIO;
using Backlogs.Utils;
using UnhandledExceptionEventArgs = Windows.UI.Xaml.UnhandledExceptionEventArgs;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;

namespace Backlogs
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private IServiceProvider m_serviceProvider;
        private IUserSettings m_userSettings;
        private INavigation m_navigationService;
        private IFileHandler m_fileHander;
        private static Frame AppFrame;
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            App.Current.UnhandledException += OnUnHandledException;
            m_serviceProvider = ConfigureServices();
        }

        public static IServiceProvider Services
        {
            get
            {
                IServiceProvider serviceProvider = ((App)Current).m_serviceProvider;
                if (serviceProvider == null)
                {
                    throw new InvalidOperationException("Service is not initialized");
                }
                return serviceProvider;
            }
        }

        public static TEnum GetEnum<TEnum>(string text) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
            }
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
                m_serviceProvider = ConfigureServices();
                m_userSettings = Services.GetRequiredService<IUserSettings>();
                m_userSettings.UserSettingsChanged += M_userSettings_UserSettingsChanged;
                rootFrame.ActualThemeChanged += RootFrame_ActualThemeChanged;
                m_fileHander = Services.GetRequiredService<IFileHandler>();
                m_navigationService = Services.GetRequiredService<INavigation>();
                m_navigationService.RegisterViewForViewModel(typeof(MainViewModel), typeof(MainPage));
                m_navigationService.RegisterViewForViewModel(typeof(BacklogsViewModel), typeof(BacklogsPage));
                m_navigationService.RegisterViewForViewModel(typeof(BacklogViewModel), typeof(BacklogPage));
                m_navigationService.RegisterViewForViewModel(typeof(CompletedBacklogsViewModel), typeof(CompletedBacklogsPage));
                m_navigationService.RegisterViewForViewModel(typeof(CompletedBacklogViewModel), typeof(CompletedBacklogPage));
                m_navigationService.RegisterViewForViewModel(typeof(CreateBacklogViewModel), typeof(CreatePage));
                m_navigationService.RegisterViewForViewModel(typeof(ImportBacklogViewModel), typeof(ImportBacklog));
                m_navigationService.RegisterViewForViewModel(typeof(SettingsViewModel), typeof(SettingsPage));

                m_navigationService.SetAnimations();
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter


                    var version = Package.Current.Id.Version;
                    string currVer = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                    var settingsService = Services.GetRequiredService<IUserSettings>();
                    var settingsVersion = settingsService.Get<string>(SettingsConstants.Version);
                    if (settingsVersion == null || settingsVersion != currVer)
                    {
                        settingsService.Set(SettingsConstants.ShowWhatsNew, true);
                    }
                    settingsService.Set(SettingsConstants.Version, currVer);
                    BacklogsManager.GetInstance().InitBacklogsManager(m_fileHander);
                    Task.Run(async () => { await BacklogsManager.GetInstance().ReadDataAsync(); }).Wait();
                    BacklogsManager.GetInstance().ResetHelperBacklogs();
                    rootFrame.Navigate(typeof(MainPage), "sync");
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

            AppFrame = rootFrame;
            SetAppTheme();
        }

        private void RootFrame_ActualThemeChanged(FrameworkElement sender, object args)
        {
            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = AppFrame.ActualTheme == ElementTheme.Dark ? Colors.White : Colors.Black;
        }

        private void M_userSettings_UserSettingsChanged(object sender, string e)
        {
            if(e == SettingsConstants.AppTheme)
            {
                SetAppTheme();
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            var provider = new ServiceCollection()
                .AddSingleton<IUserSettings, Settings>()
                .AddSingleton<IMsal, MSAL>()
                .AddSingleton<IDialogHandler, DialogHandler>()
                .AddSingleton<IEmailService, EmailHandler>()
                .AddSingleton<IFileHandler, Backlogs.Utils.UWP.FileIO>()
                .AddSingleton<IFilePicker, FilePickerService>()
                .AddSingleton<ILiveTileService, LiveTileManager>()
                .AddSingleton<IShareDialogService, ShareDialogService>()
                .AddSingleton<IToastNotificationService, ToastNotificationService>()
                .AddSingleton<INavigation, Navigator>()
                .AddSingleton<ISystemLauncher, SystemLauncher>()
                .BuildServiceProvider();
            return provider;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            InitFrame(args);

            base.OnActivated(args);
        }

        // Event fired when a Background Task is activated (in Single Process Model)
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
        }

        private void SetAppTheme()
        {
            object themeObject = m_userSettings.Get<string>(SettingsConstants.AppTheme);
            if (themeObject != null && AppFrame != null)
            {
                string theme = themeObject.ToString();
                switch(theme)
                {
                    case "Light":
                        AppFrame.RequestedTheme = ElementTheme.Light;
                        break;
                    case "Dark":
                        AppFrame.RequestedTheme = ElementTheme.Dark;
                        break;
                    default:
                        AppFrame.RequestedTheme = ElementTheme.Default;
                        break;
                }
            }
            else
            {
                m_userSettings.Set<string>(SettingsConstants.AppTheme, "System");
            }
        }

        /// <summary>
        /// Initialized root frame and navigates to the main page
        /// </summary>
        /// <param name="args"></param>
        private void InitFrame(IActivatedEventArgs args)
        {
            Frame rootFrame = GetRootFrame();

            rootFrame.Navigate(typeof(MainPage), "sync");
        }

        /// <summary>
        /// Gets the root frame. Used for setting the app theme at launch
        /// </summary>
        /// <returns>The root frame</returns>
        private Frame GetRootFrame()
        {
            Frame rootFrame;
            if (!(Window.Current.Content is MainPage rootPage))
            {
                rootPage = new MainPage();
                rootFrame = (Frame)rootPage.FindName("rootFrame");
                if (rootFrame == null)
                {
                    throw new Exception("Root frame not found");
                }
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                rootFrame.NavigationFailed += OnNavigationFailed;

                Window.Current.Content = rootPage;
            }
            else
            {
                rootFrame = (Frame)rootPage.FindName("rootFrame");
            }

            return rootFrame;
        }

        private void OnUnHandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["LastCrashLog"] = unhandledExceptionEventArgs.Exception.ToString();
        }

        protected async override void OnFileActivated(FileActivatedEventArgs args)
        {
            // TODO: Handle file activation
            // The number of files received is args.Files.Count
            // The name of the first file is args.Files[0].Name
            if (args.Files.Count > 0)
            {
                StorageFile storageFile = args.Files[0] as StorageFile;
                StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
                await storageFolder.CreateFileAsync(storageFile.Name, CreationCollisionOption.ReplaceExisting);
                string json = await FileIO.ReadTextAsync(storageFile);
                var file = await storageFolder.GetFileAsync(storageFile.Name);
                await FileIO.WriteTextAsync(file, json);
                Frame rootFrame = Window.Current.Content as Frame;

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();

                    rootFrame.NavigationFailed += OnNavigationFailed;
                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }


                rootFrame.Navigate(typeof(ImportBacklog), file.Name);
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }
    }
}
