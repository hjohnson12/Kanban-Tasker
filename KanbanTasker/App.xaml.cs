using KanbanTasker.ViewModels;
using KanbanTasker.Views;
using Microsoft.Services.Store.Engagement;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Collections.Generic;
using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using KanbanTasker.Model;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using KanbanTasker.Helpers.Microsoft_Graph.Authentication;
using Microsoft.Graph;
using Application = Windows.UI.Xaml.Application;
using KanbanTasker.Services;
using KanbanTasker.Helpers.Microsoft_Graph;
using KanbanTasker.Helpers;

namespace KanbanTasker
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 
        public static IContainer container;
        private string[] scopes = new string[] { "files.readwrite", "user.read" };
        private string appId = "422b281b-be2b-4d8a-9410-7605c92e4ff1";
        private static AuthenticationProvider authProvider;
        public AuthenticationProvider AuthProvider { get; set; }
        public static User CurrentUser { get; set; }

        public App()
        {
            // Register SyncfusionLicense
            // Check to see if API KEY is in your Environment Variables
            //if (Environment.GetEnvironmentVariable("SYNCFUSION_API_KEY") == null)
            //{
            //    // If key is not available, use free community license here: https://www.syncfusion.com/products/communitylicense
            //    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTMwNjE1QDMxMzcyZTMyMmUzMEFlSlpZMDNRQVFhUy9pOHQ4dzlObVNNbGNsQ3I2bE15NE50U2dzQ1lYK1k9");
            //}
            //else
            //{
            //    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
            //        Environment.GetEnvironmentVariable("SYNCFUSION_API_KEY"));
            //}

            // Added because was still prompting users from the store
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDU1NTIwQDMxMzkyZTMxMmUzME9UVExhQlpERC95UExwQlhRWVVtVm1mSmN0Z1ZWc3BzYU5oQmtYR0tyVk09");

            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // Build the Autofac container
            IEnumerable<IEndPointConfiguration> endPoints = EndPointUtilities.LoadEndPoints("EndPoints.json");
            string fileRoot = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            // Commented out because I'm currently not running MySQL
            // KanbanTasker.Services.ConnectionstringUtility.PopulateConnectionStrings(fileRoot, endPoints); 
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new LeaderAnalytics.AdaptiveClient.EntityFrameworkCore.AutofacModule());
            builder.RegisterModule(new AutofacModule());
            builder.RegisterModule(new Services.AutofacModule());

            RegistrationHelper registrationHelper = new RegistrationHelper(builder);
            registrationHelper
                .RegisterEndPoints(endPoints)
                .RegisterModule(new AdaptiveClientModule());
            container = builder.Build();

            IDatabaseUtilities databaseUtilities = container.Resolve<IDatabaseUtilities>();
            
            // Create all databases or apply migrations
            foreach (IEndPointConfiguration ep in endPoints.Where(x => x.EndPointType == EndPointType.DBMS))
                Task.Run(() => databaseUtilities.CreateOrUpdateDatabase(ep)).Wait();

            // Configure AppCenter for debugging analytics
            AppCenter.Start("a57ee001-5ab0-46f5-aa5a-4d1b84cd6b66",
                   typeof(Analytics), typeof(Crashes));
        }

        /// <summary>
        /// Returns the current instance of MainViewModel.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="appNotificationService"></param>
        /// <param name="dialogService"></param>
        /// <returns></returns>
        public static MainViewModel GetViewModel(Frame frame, IAppNotificationService appNotificationService, IDialogService dialogService)
        {
            return container.Resolve<MainViewModel>(
                new TypedParameter(typeof(Frame), frame),
                new TypedParameter(typeof(IAppNotificationService), appNotificationService),
                new TypedParameter(typeof(IDialogService), dialogService));
        }
        
        /// <summary>
        /// Returns the current instance of the AuthenticationProvider.
        /// </summary>
        /// <returns></returns>
        public static AuthenticationProvider GetAuthenticationProvider()
        {
            return authProvider;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
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
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainView), e.Arguments);
                }

                // Ensure the current window is active
                Window.Current.Activate();

                TitleBarHelper.ExpandViewIntoTitleBar();
                TitleBarHelper.SetupTitleBar();

                await SetupStoreServices();

                await GetCurrentUserIfSignedIn();

                await CheckForUpdateOrFirstRun();
            }
        }

        /// <summary>
        /// Registers notifications for the store services
        /// </summary>
        /// <returns></returns>
        public async Task SetupStoreServices()
        {
            var engagementManager = StoreServicesEngagementManager.GetDefault();
            await engagementManager.RegisterNotificationChannelAsync();
        }

        /// <summary>
        /// Gets the current user from Microsoft Graph if they are still authenticated with the application.
        /// </summary>
        public async Task GetCurrentUserIfSignedIn()
        {
            authProvider = new AuthenticationProvider(appId, scopes);

            GraphServiceHelper.InitializeClient(authProvider);

            var account = await authProvider.GetSignedInUser();
            if (account != null)
            {
                await authProvider.GetAccessToken();
                CurrentUser = await GraphServiceHelper.GetMeAsync();
            }
        }

        /// <summary>
        /// Checks if the app is the first time ran or updated,
        /// displays a dialog accordingly
        /// </summary>
        /// <returns></returns>
        public async Task CheckForUpdateOrFirstRun()
        {
            IDialogService dialogService = container.Resolve<IDialogService>();

            if (SystemInformation.Instance.IsAppUpdated)
            {
                await dialogService.ShowAppUpdatedDialog();
            }
            else if (SystemInformation.Instance.IsFirstRun)
            {
                await dialogService.ShowFirstRunDialog();
            }
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
    }
}