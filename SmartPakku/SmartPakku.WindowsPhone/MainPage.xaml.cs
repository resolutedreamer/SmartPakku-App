using SmartPakku.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace SmartPakku
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //this.navigationHelper.OnNavigatedTo(e);

            // MUST ENABLE THE LOCATION CAPABILITY
            // First, find the current location

            var locator = new Geolocator();
            locator.DesiredAccuracyInMeters = 50;

            Geoposition my_position = await locator.GetGeopositionAsync();
            Geopoint my_point = my_position.Coordinate.Point;
            await locatorMap.TrySetViewAsync(my_point, 18D);

            // Second, place an icon at the current location

            MapIcon IamHere = new MapIcon();
            IamHere.Location = my_point;
            IamHere.NormalizedAnchorPoint = new Point(0.5, 1.0);
            IamHere.Title = "Current Location";
            locatorMap.MapElements.Add(IamHere);

            

        }




        // pack assistant

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Being Worn!";
            Location.Text = "Dynamically Updating!";
            Recommendation.Text = "Do Nothing!";
        }

        // for now we use a button to get the informatiom from the arduino via bluetooth le
        // not entirely sure how to send the data yet. may send the data over UART
        // not sure how to define the data yet. if all processing is done on the arudino
        // then we just send the states back here.


        // example: custom bluetooth LE characteristics
        // 4 bytes

        // first byte: not sure actually
        // bit 0: connected [1] or not [0]
        // bit 1: backpack is worn [1] or stationary [0]
        // bit 2:
        // bit 3: 
        // bit 4:
        // bit 5:
        // bit 6:
        // bit 7:


        // second byte: battery level
        // bit 0: need to charge [1] or not [0]
        // bit 1: 
        // bit 2: 1-7 will be the digits for the charge percentage
        // bit 3: 2^7 = 128, maximum charge percentage is 100%
        // bit 4:
        // bit 5:
        // bit 6:
        // bit 7:


        // third byte: worn backpack
        // bit 0: left shoulder - worn[1] or not [0]
        // bit 1: right shoulder - worn[1] or not [0]
        // bit 2: left shoulder - worn[1] or not [0]
        // bit 3: right shoulder - worn[1] or not [0]
        // bit 4: left back - worn[1] or not [0]
        // bit 5: right back - worn[1] or not [0]
        // bit 6:
        // bit 7:


        // fourth byte: stationary backpack
        // bit 0: normal
        // bit 1: face down
        // bit 2: back down
        // bit 3: left side
        // bit 4: right side
        // bit 5:
        // bit 6:
        // bit 7:


        // fifth byte:
        // bit 0: connected [1] or not [0]
        // bit 1: backpack is worn [1] or stationary [0]
        // bit 2:
        // bit 3: 
        // bit 4:
        // bit 5:
        // bit 6:
        // bit 7:


        // sixth byte:
        // bit 0: connected [1] or not [0]
        // bit 1: backpack is worn [1] or stationary [0]
        // bit 2:
        // bit 3: 
        // bit 4:
        // bit 5:
        // bit 6:
        // bit 7:


        // seventh byte:
        // bit 0: 
        // bit 1: 
        // bit 2:
        // bit 3: 
        // bit 4:
        // bit 5:
        // bit 6:
        // bit 7:


        // eighth byte:
        // bit 0: 
        // bit 1: 
        // bit 2:
        // bit 3: 
        // bit 4:
        // bit 5:
        // bit 6:
        // bit 7:


        private void getPackStatus_Click(object sender, RoutedEventArgs e)
        {

        }



        // location

        ApplicationDataContainer saved_locations = ApplicationData.Current.LocalSettings;


        // This function will store the position at the center of the map
        // This function can be used for saving the location of the backpack
        // when called if the map happens to be open. What i actualy want to do
        // is get the real current position and save that!
        private void store_current_location(object sender, RoutedEventArgs e)
        {
            //ApplicationDataContainer locations = ApplicationData.Current.LocalSettings;

            var lat = locatorMap.Center.Position.Latitude;
            var lon = locatorMap.Center.Position.Longitude;

            saved_locations.Values["backpack-location-latitude"] = lat.ToString();
            saved_locations.Values["backpack-location-longitude"] = lon.ToString();
        }


        private async void phoneButton_Click(object sender, RoutedEventArgs e)
        {
            var myPosition = new Windows.Devices.Geolocation.BasicGeoposition();
            myPosition.Latitude = 33.845;
            myPosition.Longitude = -118.38;

            var myPoint = new Windows.Devices.Geolocation.Geopoint(myPosition);
            if (await locatorMap.TrySetViewAsync(myPoint, 10D))
            {
                // Haven't really thought that through!
            }
        }
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {

            var lat = locatorMap.Center.Position.Latitude;
            var lon = locatorMap.Center.Position.Longitude;
            positionTextBlock.Text = String.Format("{0}, {1}",
                locatorMap.Center.Position.Latitude,
                locatorMap.Center.Position.Longitude);

            // Let's additionally store the location from this button as wel
            store_current_location(sender, e);
        }

        private async void backpackButton_Click(object sender, RoutedEventArgs e)
        {
            if (saved_locations.Values.ContainsKey("backpack-location-latitude") && saved_locations.Values.ContainsKey("backpack-location-longitude"))
            {
                double lat = Convert.ToDouble(saved_locations.Values["backpack-location-latitude"].ToString());
                double lon = Convert.ToDouble(saved_locations.Values["backpack-location-longitude"].ToString());

                BasicGeoposition myPosition = new Windows.Devices.Geolocation.BasicGeoposition();
                myPosition.Latitude = lat;
                myPosition.Longitude = lon;

                Geopoint myPoint = new Windows.Devices.Geolocation.Geopoint(myPosition);
                await locatorMap.TrySetViewAsync(myPoint, 10D);
            }
            else
            {
                positionTextBlock.Text = "No Location Saved!";
            }

        }

        // battery life

        // TODO Print the number given from the LiPo Fuel Gauge to the Arduino to Bluetooth LE

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }


        // DO NOT MAKE IT ASYNC YET
        // FIRST MAKE A BUTTON TO HANDLE THE REQUEST AND DISPALY RESULT IN A TEXT BOX


        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(Settings));
            }
            catch
            {
                throw new Exception();
            }
        }

    }
}
