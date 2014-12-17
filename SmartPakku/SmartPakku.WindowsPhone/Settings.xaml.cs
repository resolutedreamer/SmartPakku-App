using SmartPakku.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


using Windows.Storage;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace SmartPakku
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        ApplicationDataContainer my_settings = ApplicationData.Current.LocalSettings;


        #region THEIRCODE
        public Settings()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            // my_settings
            if (my_settings.Values.ContainsKey("location-consent"))
            {
                GPSSwitch.IsOn = (bool)my_settings.Values["location-consent"];
            }

            if (my_settings.Values.ContainsKey("live-tiles-consent"))
            {
                LiveTilesSwitch.IsOn = (bool)my_settings.Values["live-tiles-consent"];
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        #endregion


        #region MYCODE

        

        // my_settings

        private void GPSSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var ToggleSwitchValue = GPSSwitch.IsOn;
            my_settings.Values["location-consent"] = ToggleSwitchValue;
        }

        private void LiveTilesSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var ToggleSwitchValue = LiveTilesSwitch.IsOn;
            my_settings.Values["live-tiles-consent"] = ToggleSwitchValue;
        }

        // WIZARD ACCESS
        private void SetupWizardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(Wizard1_PairDevice));
            }
            catch
            {
                throw new Exception();
            }
        }



        // ABOUT

        // CREDITS


        #endregion

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (my_settings.Values.ContainsKey("location-consent"))
            {
                my_settings.Values.Remove("location-consent");
            }
            if (my_settings.Values.ContainsKey("live-tiles-consent"))
            {
                my_settings.Values.Remove("live-tiles-consent");
            }
            if (my_settings.Values.ContainsKey("setup-wizard-complete"))
            {
                my_settings.Values.Remove("setup-wizard-complete");
            }
            if (my_settings.Values.ContainsKey("backpack-location-latitude"))
            {
                my_settings.Values.Remove("backpack-location-latitude");
            }
            if (my_settings.Values.ContainsKey("backpack-location-latitude"))
            {
                my_settings.Values.Remove("backpack-location-longitude");
            }

            try
            {
                Frame.Navigate(typeof(MainPage));
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}
