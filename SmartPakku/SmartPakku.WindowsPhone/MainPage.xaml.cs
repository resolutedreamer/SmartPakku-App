﻿using SmartPakku.Common;
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
using Windows.UI.Core;
using Windows.UI.Popups;
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
        ApplicationDataContainer my_settings = ApplicationData.Current.LocalSettings;

        private Geolocator locator = null;
        private CoreDispatcher _cd;

        


        public MainPage()
        {
            this.InitializeComponent();
            _cd = Window.Current.CoreWindow.Dispatcher;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.ToString() == "no-refunds")
            {
                Frame.BackStack.Clear();
            }

            Geoposition my_position;
            Geopoint my_point;
            MapIcon IamHere;

            BasicGeoposition backpack_position;
            Geopoint backpack_point;
            MapIcon BackpackHere;

            // Assuming the user has gone through the setup wizard, the following
            // code should never appear
            // jk this replaces wizard 3


            if ( ! my_settings.Values.ContainsKey("location-consent") )
            {
                // User not yet has opted in or out of Location
                // get ready to show the prompt

                //Creating instance for the MessageDialog Class  
                //and passing the message in it's Constructor               
                MessageDialog msgbox = new MessageDialog("This app accesses your phone's location. Is that ok?", "Location");

                // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
                msgbox.Commands.Add( new UICommand("Yes",
                    new UICommandInvokedHandler(this.CommandInvokedHandler)));
                msgbox.Commands.Add( new UICommand("No",
                    new UICommandInvokedHandler(this.CommandInvokedHandler)) );

                // Set the command that will be invoked by default
                msgbox.DefaultCommandIndex = 0;

                // Set the command to be invoked when escape is pressed
                msgbox.CancelCommandIndex = 1;

                // Show the message dialog
                await msgbox.ShowAsync();
            }


            // they've gone through the setup wizard, and they have the
            // location capability to be either on or off

            // now check if they've allowed location or not
            bool location_allowed = (bool)my_settings.Values["location-consent"];
            
            if (!location_allowed)
            {
                // location is not allowed
                // disable the locator and return immediately
                // thereby skipping over all of the code involving
                // loading up the mapping service.
                locator_pivot.Visibility = Visibility.Collapsed;
                return;
            }
            // otherwise continue

            // Set up the locator
            // MUST ENABLE THE LOCATION CAPABILITY

            if (locator == null)
            {
                locator = new Geolocator();
            }
            if (locator != null)
            {
                // If we were able to set up the locator
                locator.DesiredAccuracyInMeters = 50;
                locator.MovementThreshold = 3.0;
                locator.PositionChanged +=
                    new TypedEventHandler<Geolocator,
                        PositionChangedEventArgs>(geo_PositionChanged);

                // First, find the current location
                my_position = await locator.GetGeopositionAsync();
                my_point = my_position.Coordinate.Point;
                await locatorMap.TrySetViewAsync(my_point, 18D);

                // Second, place an icon at the current location

                IamHere = new MapIcon();
                IamHere.Location = my_point;
                IamHere.NormalizedAnchorPoint = new Point(0.5, 1.0);
                IamHere.Title = "Current Location";
                locatorMap.MapElements.Add(IamHere);

                // Third, get the backpack position

                backpack_position = new Windows.Devices.Geolocation.BasicGeoposition();
                if (!my_settings.Values.ContainsKey("backpack-location-latitude") 
                    || !my_settings.Values.ContainsKey("backpack-location-longitude"))
                // if latitude was not saved or longitude was not saved, save both now, very slightly off the current position.
                {
                    my_settings.Values["backpack-location-latitude"] = my_position.Coordinate.Point.Position.Latitude + .00001;
                    my_settings.Values["backpack-location-longitude"] = my_position.Coordinate.Point.Position.Longitude + .00001;
                }

                backpack_position.Latitude = (double)my_settings.Values["backpack-location-latitude"];
                backpack_position.Longitude = (double)my_settings.Values["backpack-location-longitude"];
                


                // Third part 2, turn the position into a geopoint
                backpack_point = new Windows.Devices.Geolocation.Geopoint(backpack_position);

                // fourth, place an icon where the backpack should be

                BackpackHere = new MapIcon();
                BackpackHere.Location = backpack_point;
                BackpackHere.NormalizedAnchorPoint = new Point(0.5, 1.0);
                BackpackHere.Title = "Backpack Location";
                locatorMap.MapElements.Add(BackpackHere);

            }
        }


        async private void geo_PositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await _cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Geoposition pos = e.Position;
            }
            
            );
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            // Display message showing the label of the command that was invoked
            //rootPage.NotifyUser("The '" + command.Label + "' command has been selected.",
            //    NotifyType.StatusMessage);


            var which_command = command.Label.ToString();
            if (which_command == "Yes")
            {
                my_settings.Values["location-consent"] = true;
            }
            else if (which_command == "No")
            {
                my_settings.Values["location-consent"] = false;
            }


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







        // This function will store the position at the center of the map
        // This function can be used for saving the location of the backpack
        // when called if the map happens to be open. What i actualy want to do
        // is get the real current position and save that!
        private async void store_current_location(object sender, RoutedEventArgs e)
        {
            Geoposition my_position = await locator.GetGeopositionAsync();

            var lat = locatorMap.Center.Position.Latitude;
            var lon = locatorMap.Center.Position.Longitude;

            my_settings.Values["backpack-location-latitude"] = lat.ToString();
            my_settings.Values["backpack-location-longitude"] = lon.ToString();
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

            // Let's additionally store the location from this button as well
            store_current_location(sender, e);
        }

        private async void backpackButton_Click(object sender, RoutedEventArgs e)
        {
            if (my_settings.Values.ContainsKey("backpack-location-latitude") && my_settings.Values.ContainsKey("backpack-location-longitude"))
            {
                double lat = Convert.ToDouble(my_settings.Values["backpack-location-latitude"].ToString());
                double lon = Convert.ToDouble(my_settings.Values["backpack-location-longitude"].ToString());

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
