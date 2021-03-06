﻿using HockeyApp.Internal;
using HockeyApp.ViewModels;
using HockeyApp.Tools;
using HockeyApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace HockeyApp
{
    public static class HockeyClientExtensionsWin81
    {

        #region Configure
        /// <summary>
        /// This is the main configuration method. Call this in the Constructor of your app. This registers an error handler for unhandled errors.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="appIdentifier">Your unique app id from HockeyApp.</param>
        /// <returns>Configurable Hockey client. Configure additional settings by calling methods on the returned IHockeyClientConfigurable</returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string appIdentifier)
        {
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelper81();
            @this.AsInternal().AppIdentifier = appIdentifier;

            TaskScheduler.UnobservedTaskException += async (sender, e) =>
            {
                e.SetObserved();
                await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
                if (customUnobservedTaskExceptionFunc == null || customUnobservedTaskExceptionFunc(e))
                {
                    Application.Current.Exit();
                }
            };

            Application.Current.UnhandledException += async (sender, e) =>
            {
                e.Handled = true;
                await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
                if (customUnhandledExceptionFunc == null || customUnhandledExceptionFunc(e))
                {
                    Application.Current.Exit();
                }
            };

            
            return @this as IHockeyClientConfigurable;
        }

        private static Func<UnhandledExceptionEventArgs, bool> customUnhandledExceptionFunc;

        private static Func<UnobservedTaskExceptionEventArgs, bool> customUnobservedTaskExceptionFunc;

        /// <summary>
        /// The func you set will be called after HockeyApp has written the crash-log and allows you to continue
        /// If the func returns false the app will not terminate but keep running
        /// </summary>
        /// <param name="this"></param>
        /// <param name="customFunc"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnhandledExceptionLogic(this IHockeyClientConfigurable @this, Func<UnhandledExceptionEventArgs, bool> customFunc)
        {
            customUnhandledExceptionFunc = customFunc;
            return @this;
        }

        /// <summary>
        /// The func you set will be called after HockeyApp has written the crash-log and allows you to continue
        /// If the func returns false the app will not terminate but keep running
        /// </summary>
        /// <param name="this"></param>
        /// <param name="customFunc"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnobserveredTaskExceptionLogic(this IHockeyClientConfigurable @this, Func<UnobservedTaskExceptionEventArgs, bool> customFunc)
        {
            customUnobservedTaskExceptionFunc = customFunc;
            return @this;
        }


        #endregion

        #region Feedback

        /// <summary>
        /// Invoke this method to open the feedback UI where a user can send you a message including image attachments over the HockeyApp feedback system.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="initialUserName">[Optional] Username to prefill the name field</param>
        /// <param name="initialEMail">[Optional] Email to prefill the email field</param>
        public static void ShowFeedback(this IHockeyClient @this, string initialUserName = null, string initialEMail = null)
        {
            @this.AsInternal().CheckForInitialization();
            var flyout = new FeedbackFlyout();
            FeedbackManager.Current.InitialEmail = initialEMail;
            FeedbackManager.Current.InitialUsername = initialUserName;
            flyout.ShowIndependent();
            
        }

        /// <summary>
        /// Call in your app if user logs out. Deletes all persistently stored data like FeedbackThreadToken, and cached Message. .
        /// </summary>
        public static void LogoutFromFeedback(this IHockeyClient @this)
        {
            var settingValues = ApplicationData.Current.LocalSettings.Values;
            settingValues.RemoveValue(ConstantsUniversal.FeedbackThreadKey);
            FeedbackManager.Current.CurrentFeedbackFlyoutVM = new FeedbackFlyoutVM();
        }

        #endregion

    }
}
