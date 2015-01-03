using System;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

using SmartPakkuCommon;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;
using System.Collections;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace SmartPakku
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer my_settings = ApplicationData.Current.LocalSettings;
        Geoposition pos; private Geolocator locator = null; private CoreDispatcher _cd;
        Geoposition my_position; Geopoint my_point; MapIcon my_icon; MapIcon BackpackHere;
        DeviceInformation QQ; BluetoothLEDevice bleDevice; SmartPack device;
        public static MainPage Current;

        public MainPage()
        {
            this.InitializeComponent();
            _cd = Window.Current.CoreWindow.Dispatcher;
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            bool first_run = e.ToString() == "no-refunds";
            string saved_device_id;
            string saved_device_containerid;

            Fill_HeartRate_Box_Prep();
            

            if (first_run)
            {
                Frame.BackStack.Clear();
            }

            if (!my_settings.Values.ContainsKey("location-consent"))
            {
                // User not yet has opted in or out of Location
                // so you should ask if they want to use it or not
                await location_permission_prompt();
            }

            if (my_settings.Values.ContainsKey("smartpack-device-id") && my_settings.Values.ContainsKey("smartpack-device-containerid"))
            {
                backpackStatus.Text = "Initializing...";
                saved_device_id = my_settings.Values["smartpack-device-id"].ToString();
                saved_device_containerid = my_settings.Values["smartpack-device-containerid"].ToString();

                try
                {
                    QQ = await DeviceInformation.CreateFromIdAsync(saved_device_id, new string[] { "System.Devices.ContainerId" });
                    PrepDevice(QQ);

                    bleDevice = await BluetoothLEDevice.FromIdAsync(saved_device_id);
                    device = new SmartPack(bleDevice, saved_device_containerid);

                    await device.update_battery_level();
                    await device.update_status();

                    int bat_percent = device.BatteryLevel;
                    int status = device.Status;

                    update_battery_page(bat_percent);
                    update_adjustments_page(status);

                    backpackStatus.Text = "Connected!";

                    //update_adjustments_page(device.Status);
                    update_adjustments_page(1);
                }
                catch
                {
                    backpackStatus.Text = "Error connecting to SmartPack.\nPlease repeat the setup wizard."; 
                }
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


            //List_HeartRate_Devices();
            
            

            await map_init(first_run);
        }


#region Adjustments
        // Adjustments Pivot
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int status = 1;
            update_adjustments_page(status);
        }

        private void getPackStatus_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Scenario1_DeviceEvents));
            int status = 1;
            //int status = device.Status;
            update_adjustments_page(status);
        }

        private void update_adjustments_page(int status)
        {
            switch (status)
            {
                case 1:
                    Status.Text = "Being Worn!";
                    var x = new BitmapImage(new Uri("ms-appx:///Assets/backpack-wearing.png"));
                    StatusImage.Source = x;
                    Location.Text = "With both shoulders";
                    Recommendation.Text = "Do Nothing!";
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    break;
                default:
                    break;
            }
        }

        #region HeartRateStuff
        private void Fill_HeartRate_Box_Prep()
        {
            bool z = HeartRateService.Instance.IsServiceInitialized;
            if (z)
            {
                foreach (var measurement in HeartRateService.Instance.DataPoints)
                {
                    outputListBox.Items.Add(measurement.ToString());
                }
                outputGrid.Visibility = Visibility.Visible;
            }
            HeartRateService.Instance.ValueChangeCompleted += Instance_ValueChangeCompleted;
        }

        private async void List_HeartRate_Devices()
        {
            backpackStatus.Text = "Listing SmartPack Devices";

            var devices = await DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.HeartRate),
                new string[] { "System.Devices.ContainerId" });


            DevicesListBox.Items.Clear();

            if (devices.Count > 0)
            {
                foreach (var device in devices)
                {
                    DevicesListBox.Items.Add(device);
                }
                DevicesListBox.Visibility = Visibility.Visible;
            }
            else
            {
                backpackStatus.Text = "Could not find any Heart Rate devices. Please make sure your device is paired " +
                    "and powered on!";
            }
        }

        private async void Instance_ValueChangeCompleted(HeartRateMeasurement heartRateMeasurementValue)
        {
            // Serialize UI update to the the main UI thread.
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                backpackStatus.Text = "Latest received heart rate measurement:\n" +
                    heartRateMeasurementValue.HeartRateValue;

                outputListBox.Items.Insert(0, heartRateMeasurementValue);
            });
        }

        private async void PrepDevice(DeviceInformation device)
        {
            //var device = DevicesListBox.SelectedItem as DeviceInformation;
            DevicesListBox.Visibility = Visibility.Collapsed;

            backpackStatus.Text = "Initializing device...";
            HeartRateService.Instance.DeviceConnectionUpdated += OnDeviceConnectionUpdated;
            await HeartRateService.Instance.InitializeServiceAsync(device);
            outputGrid.Visibility = Visibility.Visible;
            try
            {
                // Check if the device is initially connected, and display the appropriate message to the user


                var x = PnpObjectType.DeviceContainer;
                var y = device.Properties["System.Devices.ContainerId"].ToString();
                var z = new string[] { "System.Devices.Connected" };

                var deviceObject = await PnpObject.CreateFromIdAsync(x, y, z);

                bool isConnected;
                if (Boolean.TryParse(deviceObject.Properties["System.Devices.Connected"].ToString(), out isConnected))
                {
                    OnDeviceConnectionUpdated(isConnected);
                }
            }
            catch (Exception)
            {
                backpackStatus.Text = "Retrieving device properties failed";
            }
        }

        private async void OnDeviceConnectionUpdated(bool isConnected)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (isConnected)
                {
                    backpackStatus.Text = "Waiting for device to send data...";
                }
                else
                {
                    backpackStatus.Text = "Waiting for device to connect...";
                }
            });
        }

        private void DevicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var X = QQ;
            var device = DevicesListBox.SelectedItem as DeviceInformation;
            DevicesListBox.Visibility = Visibility.Collapsed;
            PrepDevice(device);
        }
        #endregion

#endregion

#region Location
        // Location Pivot
        private async void phoneButton_Click(object sender, RoutedEventArgs e)
        {
            // update the current point
            await update_current_location();
            // get the current point
            Geopoint myPoint = await get_current_point();
            if (myPoint == null)
            {
                statusTextBlock.Text = "myPoint is null!";
            }
            else
            {
                statusTextBlock.Text = "We got the current position:";
                await locatorMap.TrySetViewAsync(myPoint, 16D);
                positionTextBlock.Text = String.Format("{0}, {1}", myPoint.Position.Latitude, myPoint.Position.Longitude);
            }

        }
        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            //store the current location
            await store_current_location();

            //then print result to screen
            Geopoint myPoint = get_saved_backpack_location();
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

        private async void backpackButton_Click(object sender, RoutedEventArgs e)
        {
            Geopoint myPoint = get_saved_backpack_location();
            if (myPoint == null)
            {
                statusTextBlock.Text = "The point we have saved is null!";
            }
            else
            {
                await locatorMap.TrySetViewAsync(myPoint, 16D);
                bool firstrun = false;
                MapIcon BackpackHere = await get_backpack_icon(firstrun);
                if (BackpackHere == null)
                {
                    statusTextBlock.Text = "Problem with backpack icon";
                }
                else
                {
                    locatorMap.MapElements.Add(BackpackHere);
                    statusTextBlock.Text = "Retrived saved point and added icon:";
                    positionTextBlock.Text = String.Format("{0}, {1}", myPoint.Position.Latitude, myPoint.Position.Longitude);
                }
            }

        }
        // Location Page

        private async Task map_init(bool first_run)
        {
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
                my_point = await get_current_point();
                if (my_point != null)
                {
                    await locatorMap.TrySetViewAsync(my_point, 16D);
                }

                // Second, place an icon at the current location
                my_icon = get_icon();
                if (my_icon != null)
                {
                    locatorMap.MapElements.Add(my_icon);
                }

                BackpackHere = await get_backpack_icon(first_run);
                if (BackpackHere != null)
                {
                    locatorMap.MapElements.Add(BackpackHere);
                }
            }
        }

        private MapIcon get_icon()
        {
            MapIcon tmp_icon = new MapIcon();
            if (my_point != null)
            {
                tmp_icon.Location = my_point;
                tmp_icon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                tmp_icon.Title = "My Location";
                return tmp_icon;
            }
            else
            {
                return null;
            }
        }

        public async Task<MapIcon> get_backpack_icon(bool firstrun)
        {
            MapIcon tmp_backpack_icon = new MapIcon();
            if (my_settings.Values.ContainsKey("backpack-location-latitude") && my_settings.Values.ContainsKey("backpack-location-longitude"))
            {
                BasicGeoposition tmp_position = new BasicGeoposition();
                tmp_position.Latitude = (double)my_settings.Values["backpack-location-latitude"];
                tmp_position.Longitude = (double)my_settings.Values["backpack-location-longitude"];
                Geopoint tmp_point = new Geopoint(tmp_position);

                tmp_backpack_icon.Location = tmp_point;
                tmp_backpack_icon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                tmp_backpack_icon.Title = "Backpack Location";
                return tmp_backpack_icon;
            }
            else if (firstrun == true)
            {
                await store_current_location();

                BasicGeoposition tmp_position = new BasicGeoposition();
                tmp_position.Latitude = (double)my_settings.Values["backpack-location-latitude"];
                tmp_position.Longitude = (double)my_settings.Values["backpack-location-longitude"];
                Geopoint tmp_point = new Geopoint(tmp_position);

                tmp_backpack_icon.Location = tmp_point;
                tmp_backpack_icon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                tmp_backpack_icon.Title = "Backpack Location";
                return tmp_backpack_icon;
            }
            else
            {
                return null;
            }
        }

        private async Task<Geoposition> get_current_position()
        {
            await update_current_location();
            return my_position;
        }

        private async Task<Geopoint> get_current_point()
        {
            await update_current_location();
            return my_point;
        }

        private async Task update_current_location()
        {
            my_position = await locator.GetGeopositionAsync();
            my_point = my_position.Coordinate.Point;
        }
        public async Task store_current_location()
        {
            await update_current_location();
            my_settings.Values["backpack-location-latitude"] = my_position.Coordinate.Point.Position.Latitude;
            my_settings.Values["backpack-location-longitude"] = my_position.Coordinate.Point.Position.Longitude;
        }

        public Geopoint get_saved_backpack_location()
        {
            if (my_settings.Values.ContainsKey("backpack-location-latitude") && my_settings.Values.ContainsKey("backpack-location-longitude"))
            {
                BasicGeoposition myPosition = new BasicGeoposition();
                myPosition.Latitude = (double)my_settings.Values["backpack-location-latitude"];
                myPosition.Longitude = (double)my_settings.Values["backpack-location-longitude"];

                Geopoint myPoint = new Geopoint(myPosition);
                return myPoint;
            }
            else
            {
                return null;
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
        private async Task location_permission_prompt()
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
#endregion

#region Battery
        // Battery Pivot
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int bat_percent = device.BatteryLevel;
            update_battery_page(bat_percent);
        }

        private void update_battery_page(int bat_percent)
        {
            backpackStatus.Text = "Updating Battery Level...";
            if (device.HasBatteryService)
            {                
                if ((bat_percent <= 0) || (bat_percent > 100))
                {
                    battery_percentage.Text = "???";
                    recommendation.Text = "Error: Invalid Battery Level";
                }
                else
                {
                    battery_percentage.Text = bat_percent.ToString() + " %";
                    recommendation.Text = "";
                    if (bat_percent < 8)
                    {
                        recommendation.Text = "Warning: Battery Critically Low - Charge Now!";
                    }
                    else if (bat_percent < 20)
                    {
                        recommendation.Text = "Warning: Battery Low - Consider Charging";
                    }
                }
            }
        }
        #endregion

#region AppBar
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
        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            if (TestingPanel.Visibility == Visibility.Collapsed)
            {
                TestingPanel.Visibility = Visibility.Visible;
                DevicesListBox.Visibility = Visibility.Visible;
                outputGrid.Visibility = Visibility.Visible;
            }
            else
            {
                TestingPanel.Visibility = Visibility.Collapsed;
            }
        }
#endregion

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