using KanbanTasker.ViewModels;
using KanbanTasker.Views;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using LeaderAnalytics.AdaptiveClient.Utilities;
using System.Collections.Generic;
using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using System.Threading.Tasks;
using KanbanTasker.Views;
using System.Linq;
using KanbanTasker.Model;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

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
        private static IContainer container;

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
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTMwNjE1QDMxMzcyZTMyMmUzMEFlSlpZMDNRQVFhUy9pOHQ4dzlObVNNbGNsQ3I2bE15NE50U2dzQ1lYK1k9");
           

            this.InitializeComponent();
            this.Suspending += OnSuspending;


            // build the Autofac container
            IEnumerable<IEndPointConfiguration> endPoints = EndPointUtilities.LoadEndPoints("EndPoints.json");
            string fileRoot = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            // Commented out because I'm  currently not running MySQL
           // KanbanTasker.Services.ConnectionstringUtility.PopulateConnectionStrings(fileRoot, endPoints); 
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new LeaderAnalytics.AdaptiveClient.EntityFrameworkCore.AutofacModule());
            builder.RegisterModule(new AutofacModule());
            builder.RegisterModule(new Services.AutofacModule());
            RegistrationHelper registrationHelper = new RegistrationHelper(builder);

            registrationHelper
                .RegisterEndPoints(endPoints)
                .RegisterModule(new KanbanTasker.Services.AdaptiveClientModule());


            container = builder.Build();

            IDatabaseUtilities databaseUtilities = container.Resolve<IDatabaseUtilities>();

            // Create all databases or apply migrations
                     

            foreach (IEndPointConfiguration ep in endPoints.Where(x => x.EndPointType == EndPointType.DBMS))
                Task.Run(() => databaseUtilities.CreateOrUpdateDatabase(ep)).Wait();

            AppCenter.Start("a57ee001-5ab0-46f5-aa5a-4d1b84cd6b66",
                   typeof(Analytics), typeof(Crashes));
        }

        public static MainViewModel GetViewModel(Frame frame, InAppNotification messagePump)
        {
            return container.Resolve<MainViewModel>(new TypedParameter(typeof(Frame), frame), new TypedParameter(typeof(InAppNotification), messagePump));
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

                // Hide default title bar and extend your content into the title bar area
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

                var titleBar = ApplicationView.GetForCurrentView().TitleBar;

                // Set active window colors
                titleBar.ForegroundColor = Windows.UI.Colors.White;
                titleBar.BackgroundColor = Windows.UI.Colors.Transparent;
                titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
                titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.SlateGray;
                titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.DimGray;

                // Set inactive window colors
                titleBar.InactiveForegroundColor = Windows.UI.Colors.White;
                titleBar.InactiveBackgroundColor = Windows.UI.Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

                CheckForUpdateOrFirstRun();
            }
        }

        public async void CheckForUpdateOrFirstRun()
        {
            if (SystemInformation.IsAppUpdated)
            {
                var dialog = new AppUpdatedDialogView();
                var result = await dialog.ShowAsync();
            }
            else if (SystemInformation.IsFirstRun)
            {
                var dialog = new FirstRunDialogView();
                var result = await dialog.ShowAsync();
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
