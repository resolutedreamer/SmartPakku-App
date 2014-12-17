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
using SmartPakkuBackground;
using Windows.Devices.Bluetooth;
using Windows.Devices.Background;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace SmartPakku
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer my_settings = ApplicationData.Current.LocalSettings;

        Geoposition pos;
        private Geolocator locator = null;
        private CoreDispatcher _cd;

        MapperFunctions lets_map = new MapperFunctions();


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


            if (!my_settings.Values.ContainsKey("location-consent"))
            {
                // User not yet has opted in or out of Location
                // so you should ask if they want to use it or not
                location_permission_prompt();
            }

            // they've gone through the setup wizard, and they have the
            // location capability to be either on or off
            // now check if they've allowed location or not
            bool location_allowed = (bool)my_settings.Values["location-consent"];

            if (location_allowed == false)
            {
                // disable the locator and skip over
                // loading up the mapping service.
                LocatorContent.Visibility = Visibility.Collapsed;
                LocatorOff.Visibility = Visibility.Visible;
                return;
            }


            // get current location
            Geopoint my_point = lets_map.get_current_point();

            // get current location icon
            MapIcon my_icon = lets_map.get_icon();

            // get last saved backpack location icon
            MapIcon my_backpack_icon = lets_map.get_backpack_icon();

            // set map to current location
            if (my_point != null)
            {
                await locatorMap.TrySetViewAsync(my_point);
            }
            
            // place current location icon
            if (my_icon != null)
            {
                locatorMap.MapElements.Add(my_icon);
            }


            // place icon for last saved backpack location
            if (my_backpack_icon != null)
            {
                locatorMap.MapElements.Add(my_backpack_icon);
            }

            //old_map_update();
        }


        async void old_map_update()
        {
            Geoposition my_position;
            Geopoint my_point;
            MapIcon my_icon;

            BasicGeoposition backpack_position;
            Geopoint backpack_point;
            MapIcon BackpackHere;


            // Set up the locator
            if (locator == null)
            {
                locator = new Geolocator();
            }
            if (locator != null)
            {
                // If we were able to set up the locator
                locator.DesiredAccuracyInMeters = 50;
                //locator.ReportInterval = 30000;
                locator.MovementThreshold = 3.0;
                locator.PositionChanged +=
                    new TypedEventHandler<Geolocator,
                        PositionChangedEventArgs>(geo_PositionChanged);

                // First, find the current location
                my_position = await locator.GetGeopositionAsync();
                my_point = my_position.Coordinate.Point;
                await locatorMap.TrySetViewAsync(my_point, 18D);

                // Second, place an icon at the current location

                my_icon = new MapIcon();
                my_icon.Location = my_point;
                my_icon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                my_icon.Title = "Current Location";
                locatorMap.MapElements.Add(my_icon);

                // Third, get the backpack position

                backpack_position = new Windows.Devices.Geolocation.BasicGeoposition();
                if (!my_settings.Values.ContainsKey("backpack-location-latitude")
                    || !my_settings.Values.ContainsKey("backpack-location-longitude"))
                // if latitude was not saved or longitude was not saved, save both now, very slightly off the current position.
                {
                    my_settings.Values["backpack-location-latitude"] = my_position.Coordinate.Point.Position.Latitude + .000000001;
                    my_settings.Values["backpack-location-longitude"] = my_position.Coordinate.Point.Position.Longitude + .000000002;
                }

                backpack_position.Latitude = (double)my_settings.Values["backpack-location-latitude"];
                backpack_position.Longitude = (double)my_settings.Values["backpack-location-longitude"];



                // Third part 2, turn the position into a geopoint
                backpack_point = new Windows.Devices.Geolocation.Geopoint(backpack_position);

                // fourth, place an icon where the backpack should be

                BackpackHere = new MapIcon();
                BackpackHere.Location = backpack_point;
                BackpackHere.NormalizedAnchorPoint = new Point(0.5, 1.0);
                BackpackHere.Title = "Super Backpack Location";
                locatorMap.MapElements.Add(BackpackHere);

            }
        }

        async private void geo_PositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await _cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                pos = e.Position;
            }

            );
        }

        // pack assistant

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Status.Text = "Being Worn!";
            Location.Text = "Dynamically Updating!";
            Recommendation.Text = "Do Nothing!";
        }


        private void getPackStatus_Click(object sender, RoutedEventArgs e)
        {

        }



        // location

        private async void phoneButton_Click(object sender, RoutedEventArgs e)
        {
            // update the current point
            await lets_map.update_current_location();
            // get the current point
            Geopoint myPoint = await lets_map.get_current_point();
            if (myPoint == null)
            {
                statusTextBlock.Text = "myPoint is null!";
            }
            else
            {
                statusTextBlock.Text = "We got the current position:";
                await locatorMap.TrySetViewAsync(myPoint, 10D);
                positionTextBlock.Text = String.Format("{0}, {1}", myPoint.Position.Latitude, myPoint.Position.Longitude);
            }

        }
        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            // update the current location
            await lets_map.update_current_location();
            //store the current location
            lets_map.store_current_location();

            //then print result to screen
            Geopoint myPoint = lets_map.get_saved_backpack_location();
            if (myPoint == null)
            {
                statusTextBlock.Text = "The point we have saved is null!";
            }
            else
            {
                statusTextBlock.Text = "The Point we have saved is:";
                positionTextBlock.Text = String.Format("{0}, {1}", myPoint.Position.Latitude, myPoint.Position.Longitude);
            }
        }

        private void backpackButton_Click(object sender, RoutedEventArgs e)
        {
            Geopoint myPoint = lets_map.get_saved_backpack_location();
            if (myPoint == null)
            {
                statusTextBlock.Text = "The point we have saved is null!";
            }
            else
            {
                MapIcon BackpackHereAsWell = new MapIcon();
                BackpackHereAsWell.Location = myPoint;
                BackpackHereAsWell.NormalizedAnchorPoint = new Point(0.5, 1.0);
                BackpackHereAsWell.Title = "Backpack Location";
                

                MapIcon BackpackHere = lets_map.get_backpack_icon();
                if (BackpackHereAsWell == null)
                {
                    statusTextBlock.Text = "Problem with backpack icon";
                }
                else
                {
                    locatorMap.MapElements.Add(BackpackHereAsWell);
                    statusTextBlock.Text = "Retrived saved point and added icon:";
                    positionTextBlock.Text = String.Format("{0}, {1}", myPoint.Position.Latitude, myPoint.Position.Longitude);
                }
            }
            
        }


        // battery life

        // TODO Print the number given from the LiPo Fuel Gauge to the Arduino to Bluetooth LE

        // DO NOT MAKE IT ASYNC YET
        // FIRST MAKE A BUTTON TO HANDLE THE REQUEST AND DISPALY RESULT IN A TEXT BOX
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }


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



        private async void location_permission_prompt()
        {
            //Creating instance for the MessageDialog Class  
            //and passing the message in it's Constructor               
            MessageDialog msgbox = new MessageDialog("This app accesses your phone's location. Is that ok?", "Location");

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            msgbox.Commands.Add(new UICommand("Yes",
                new UICommandInvokedHandler(this.CommandInvokedHandler)));
            msgbox.Commands.Add(new UICommand("No",
                new UICommandInvokedHandler(this.CommandInvokedHandler)));

            // Set the command that will be invoked by default
            msgbox.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            msgbox.CancelCommandIndex = 1;

            // Show the message dialog
            await msgbox.ShowAsync();
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
    }
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