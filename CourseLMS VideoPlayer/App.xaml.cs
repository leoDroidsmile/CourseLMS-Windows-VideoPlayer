﻿using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CourseLMS_VideoPlayer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        string userId = "";
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
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
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
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

        string ServerUrl = "http://localhost/api/v1";

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                // TODO: Handle URI activation
                // The received URI is eventArgs.Uri.AbsoluteUri

                Dictionary<string, string> query = ParseQueryString(eventArgs.Uri.OriginalString);
                string user = query["user"];
                string content = query["class"];
                userId = user;

                // Call API
                string uri = "http://lms.olmaa.net/api/v1/contentWithVideo?userId=" + user + "&contentId=" + content;
                GetVideoData(uri);
            }
        }

        public static Dictionary<string, string> ParseQueryString(string uri)
        {

            string substring = uri.Substring(((uri.LastIndexOf('?') == -1) ? 0 : uri.LastIndexOf('?') + 1));

            string[] pairs = substring.Split('&');

            Dictionary<string, string> output = new Dictionary<string, string>();

            foreach (string piece in pairs)
            {
                string[] pair = piece.Split('=');
                output.Add(pair[0], pair[1]);
            }

            return output;

        }

        public async Task GetVideoData(string uri)
        {
            var httpClient = new HttpClient();

            Uri requestUri = new Uri(uri);
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";

            try
            {
                //Send the GET request
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

                JObject jObject = JObject.Parse(httpResponseBody);
                string type = (string) jObject["provider"];
                string url = (string)jObject["url"];


                string htmlContent = "<body style=\"display: flex;  justify-content: center;  align-items: center;\" >";

                if (type.Contains("Youtube"))
                    htmlContent += string.Format("<iframe width=\"100%\" src=\"{0}\" frameborder=\"0\" allow=\"accelerometer; autoplay; encrypted - media; gyroscope; picture-in-picture\" allowfullscreen></iframe>", url);
                if (type.Contains("Vimeo"))
                    htmlContent += string.Format("<iframe src=\"https://player.vimeo.com/video/{0}\" frameborder=\"0\" allow=\"autoplay; fullscreen\" allowfullscreen></iframe>", url);
                if (type.Contains("HTML5") || type.Contains("file"))
                    htmlContent += string.Format("<video width=\"100%\" controls playsinline id=\"player\" class=\"html-video-frame\" src=\"{0}\" type=\"video/mp4\"></video>", url);

                htmlContent += string.Format("<marquee width=\"100%\" behavior=\"scroll\" direction=\"left\" scrollamount=\"1\" style=\"top: 50%; position: absolute; color: gray; font - size: 40px; \">{0}</marquee>", userId);
                htmlContent += "</body>";

                MainPage.webViewMainContent.NavigateToString(htmlContent);
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
        }
    }
}
