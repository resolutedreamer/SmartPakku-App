using SmartPakku.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace SmartPakku
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Wizard4_PersonalInfo : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        ApplicationDataContainer myProfile = ApplicationData.Current.LocalSettings;
        ApplicationDataContainer permissions = ApplicationData.Current.LocalSettings;



        public Wizard4_PersonalInfo()
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
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

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


        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(DevicePage), "CLEARBACKSTACK");
            }
            catch
            {
                throw new Exception();
            }
        }

        
    }
}
