using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartPakkuCommon;


//  Make it obvious which namespace provided each referenced type:
using BackgroundTaskDeferral = Windows.ApplicationModel.Background.BackgroundTaskDeferral;
using BluetoothConnectionStatus = Windows.Devices.Bluetooth.BluetoothConnectionStatus;
using BluetoothLEDevice = Windows.Devices.Bluetooth.BluetoothLEDevice;
using DeviceConnectionChangeTriggerDetails = Windows.Devices.Enumeration.DeviceConnectionChangeTriggerDetails;
using DeviceInformation = Windows.Devices.Enumeration.DeviceInformation;
using IBackgroundTask = Windows.ApplicationModel.Background.IBackgroundTask;
using ToastNotificationManager = Windows.UI.Notifications.ToastNotificationManager;
using ToastNotifier = Windows.UI.Notifications.ToastNotifier;
using ToastNotification = Windows.UI.Notifications.ToastNotification;
using ToastTemplateType = Windows.UI.Notifications.ToastTemplateType;
using XmlDocument = Windows.Data.Xml.Dom.XmlDocument;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Storage;

///////////////////////////////////////////////////////////using SmartPack = KeepTheKeysCommon.SmartPack;


namespace SmartPakkuBackground
{
    public sealed class ConnectivityTask : IBackgroundTask
    {

        ApplicationDataContainer my_settings = ApplicationData.Current.LocalSettings;
        Geoposition my_position;
        Geopoint my_point;
        Geolocator locator;

        public async void Run(Windows.ApplicationModel.Background.IBackgroundTaskInstance taskInstance)
        {

            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            try
            {
                DeviceConnectionChangeTriggerDetails details = (DeviceConnectionChangeTriggerDetails)taskInstance.TriggerDetails;
                BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromIdAsync(details.DeviceId);
                SmartPack device = new SmartPack(bleDevice);


                if (bleDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
                    if (device.AlertOnDevice && device.HasLinkLossService)
                    {
                        await device.SetAlertLevelCharacteristic();
                    }
                }
                else
                {
                    // the device and the phone are actually disconnected

                    // now we need to determine what moved, the device or the phone
                    // we can get the current location of the phone               
                    locator = new Geolocator();
                    locator.DesiredAccuracyInMeters = 50;
                    my_point = await get_current_point();
                    while (true)
                    {
                        if (my_point != null)
                        {
                            break;
                        }
                    }
                    BasicGeoposition x = new BasicGeoposition();
                    x.Latitude = device.Latitude;
                    x.Longitude = device.Longitude;
                    Geopoint former_backpack_point = new Geopoint(x);

                    if (my_point == former_backpack_point)
                    {
                        // if the current location is the same location we last
                        // saw the backpack, that means the backpack moved and
                        // the phone did not move

                    }
                    else
                    {
                        // if the current location is different than the last
                        // location we saw the backpack, then we have moved
                        // since, so when we pull up the map, we should change
                        // our location and not the backpack location
                    }
                    store_current_location();



                    if (device.AlertOnPhone)
                    {
                        XmlDocument xml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                        xml.SelectSingleNode("/toast/visual/binding/text").InnerText = string.Format("Device {0} is out of range.", device.Name);
                        ToastNotification toast = new ToastNotification(xml);
                        ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();
                        notifier.Show(toast);
                    }
                }





            }
            finally
            {
                deferral.Complete();
            }
        }


        private async Task<Geopoint> get_current_point()
        {
            my_position = await locator.GetGeopositionAsync();
            my_point = my_position.Coordinate.Point;
            return my_point;
        }

        public void store_current_location()
        {
            my_settings.Values["backpack-location-latitude"] = my_position.Coordinate.Point.Position.Latitude;
            my_settings.Values["backpack-location-longitude"] = my_position.Coordinate.Point.Position.Longitude;
        }

    }
}
