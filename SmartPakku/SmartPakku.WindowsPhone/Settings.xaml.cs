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

        ApplicationDataContainer myProfile = ApplicationData.Current.LocalSettings;
        ApplicationDataContainer permissions = ApplicationData.Current.LocalSettings;


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



            // MY PROFILE
            if (myProfile.Values.ContainsKey("gender"))
            {
                string test = myProfile.Values["gender"].ToString();
                GenderDropDown.SelectedItem = test;
            }

            if (myProfile.Values.ContainsKey("dob_day"))
            {
                string day = myProfile.Values["dob_day"].ToString();
                string month = myProfile.Values["dob_month"].ToString();
                string year = myProfile.Values["dob_year"].ToString();

                int year_num = Convert.ToInt32(year);
                int month_num = Convert.ToInt32(month);
                int day_num = Convert.ToInt32(day);

                DateTime TheDate = new DateTime(year_num, month_num,day_num);
                DateTimeOffset saveTheDate = new DateTimeOffset(TheDate);
                DOBPicker.Date = saveTheDate;
            }

            if (myProfile.Values.ContainsKey("units"))
            {
                int selection_index = (int)myProfile.Values["units"];
                Units.SelectedIndex = selection_index;
                if (selection_index == 0)
                {
                    HeightInput.Header = "Height (ft)";
                    WeightInput.Header = "Weight (lbs)";
                }
                else if (selection_index == 1)
                {
                    HeightInput.Header = "Height (cm)";
                    WeightInput.Header = "Weight (kgs)";
                }
            }

            if (myProfile.Values.ContainsKey("weight"))
            {
                WeightInput.Text = myProfile.Values["weight"].ToString();
            }

            if (myProfile.Values.ContainsKey("height"))
            {
                string test = myProfile.Values["height"].ToString();
                HeightInput.Text = myProfile.Values["height"].ToString();
            }



            // PERMISSIONS
            if (permissions.Values.ContainsKey("location"))
            {
                GPSSwitch.IsOn = (bool)permissions.Values["location"];
            }

            if (permissions.Values.ContainsKey("live-tiles"))
            {
                LiveTilesSwitch.IsOn = (bool)permissions.Values["live-tiles"];
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        #endregion


        #region MYCODE

        // MY PROFILE

        private void GenderDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selection = GenderDropDown.SelectedItem.ToString();
            myProfile.Values["gender"] = selection;
        }

        private void DOBPicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            string day = DOBPicker.Date.Day.ToString();
            string month = DOBPicker.Date.Month.ToString();
            string year = DOBPicker.Date.Year.ToString();
            myProfile.Values["dob_day"] = day;
            myProfile.Values["dob_month"] = month;
            myProfile.Values["dob_year"] = year;
        }

        private void Units_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int previous_units = -1;
            int current_units = Units.SelectedIndex;


            // If the user has picked units before, we want to know this
            if (myProfile.Values.ContainsKey("units"))
            {
                previous_units = (int)myProfile.Values["units"];
            }
            // The user has picked units for the first time, save units
            // and allow user to input. No conversion necessary.
            else
            {
                myProfile.Values["units"] = current_units;

                if (current_units == 0)
                {
                    HeightInput.Header = "Height (ft)";
                    WeightInput.Header = "Weight (lbs)";
                }
                else if (current_units == 1)
                {
                    HeightInput.Header = "Height (cm)";
                    WeightInput.Header = "Weight (kgs)";
                }
            }

            // If the new selection is the same as the old selection, end
            if (current_units == previous_units)
            {
                return;
            }

            // User is switching from metric to imperial, convert values accordingly
            else if (current_units == 0 && previous_units == 1)
            {
                myProfile.Values["units"] = 0;
                HeightInput.Header = "Height (ft)";
                WeightInput.Header = "Weight (lbs)";

                double weight_in_metric = Convert.ToDouble(WeightInput.Text);
                double height_in_metric = Convert.ToDouble(HeightInput.Text);

                double weight_in_imperial = weight_in_metric * 2.205;
                double height_in_imperial = height_in_metric * 0.03281;

                double display_this_weight = Math.Round(weight_in_imperial, MidpointRounding.AwayFromZero);
                double display_this_height = Math.Round(height_in_imperial, 2, MidpointRounding.AwayFromZero);

                WeightInput.Text = display_this_weight.ToString();
                HeightInput.Text = display_this_height.ToString();

                myProfile.Values["weight"] = WeightInput.Text;
                myProfile.Values["height"] = HeightInput.Text;
            }

            // User is switching from imperial to metric, convert values accordingly.
            else if (current_units == 1 && previous_units == 0)
            {
                myProfile.Values["units"] = 1;
                HeightInput.Header = "Height (cm)";
                WeightInput.Header = "Weight (kgs)";

                double weight_in_imperial = Convert.ToDouble(WeightInput.Text);
                double height_in_imperial = Convert.ToDouble(HeightInput.Text);

                double weight_in_metric = (int)(weight_in_imperial * 0.4536);
                double height_in_metric = height_in_imperial * 30.48;

                double display_this_weight = Math.Round(weight_in_metric, MidpointRounding.AwayFromZero);
                double display_this_height = Math.Round(height_in_metric, 2, MidpointRounding.AwayFromZero);

                WeightInput.Text = display_this_weight.ToString();
                HeightInput.Text = display_this_height.ToString();

                myProfile.Values["weight"] = WeightInput.Text;
                myProfile.Values["height"] = HeightInput.Text;
            }


        }

        private void WeightInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            myProfile.Values["weight"] = WeightInput.Text;
        }

        private void HeightInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            myProfile.Values["height"] = HeightInput.Text;
        }


        // PERMISSIONS

        private void GPSSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var ToggleSwitchValue = GPSSwitch.IsOn;
            permissions.Values["location"] = ToggleSwitchValue;
        }

        private void LiveTilesSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var ToggleSwitchValue = LiveTilesSwitch.IsOn;
            permissions.Values["live-tiles"] = ToggleSwitchValue;
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

        
    }
}
