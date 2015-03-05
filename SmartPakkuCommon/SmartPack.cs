using System;
using System.Collections.Generic;
using System.Text;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

using System.Runtime.InteropServices.WindowsRuntime; // extension method byte[].AsBuffer()

//  Make it obvious which namespace provided each referenced type:
using ApplicationData = Windows.Storage.ApplicationData;
using ApplicationDataContainer = Windows.Storage.ApplicationDataContainer;
using BackgroundTaskBuilder = Windows.ApplicationModel.Background.BackgroundTaskBuilder;
using BackgroundTaskRegistration = Windows.ApplicationModel.Background.BackgroundTaskRegistration;
using BluetoothConnectionStatus = Windows.Devices.Bluetooth.BluetoothConnectionStatus;
using BluetoothLEDevice = Windows.Devices.Bluetooth.BluetoothLEDevice;
using DeviceConnectionChangeTrigger = Windows.ApplicationModel.Background.DeviceConnectionChangeTrigger;
using GattCharacteristic = Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic;
using GattCharacteristicUuids = Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristicUuids;
using GattDeviceService = Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceService;
using GattServiceUuids = Windows.Devices.Bluetooth.GenericAttributeProfile.GattServiceUuids;
using GattWriteOption = Windows.Devices.Bluetooth.GenericAttributeProfile.GattWriteOption;
using Task = System.Threading.Tasks.Task;
using Windows.Storage.Streams;
using Windows.ApplicationModel.Background;
using Windows.Devices.Enumeration;
using System.Threading.Tasks;

namespace SmartPakkuCommon
{
    public sealed class SmartPack
    {
        // static data
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        // data members
        private BluetoothLEDevice device;           // constant, always non-null
        private string addressString;               // constant, Bluetooth address as 12 hex digits
        private GattDeviceService linkLossService;  // constant, may be null
        private bool alertOnPhone;                  // true iff we want a popup when this device disconnects
        private bool alertOnDevice;                 // true iff we want device to alert upon disconnection
        private AlertLevel alertLevel;              // alert level that device will set upon disconnection

        private GattDeviceService batteryService;  // constant, may be null
        private GattDeviceService heartRateService;  // constant, may be null

        private double latitude, longitude;
        private int battery_level, status;
        private string device_id, device_container_id;




        // trivial properties
        public BackgroundTaskRegistration TaskRegistration { get; set; }

        // readonly properties
        public bool HasLinkLossService { get { return linkLossService != null; } }
        public string Name { get { return device.Name; } }
        public string TaskName { get { return addressString; } }

        public string DeviceId { get { return device_id; } }

        public string DeviceContainerId { get { return device_container_id; } }

        public string AddressString { get { return addressString; } }

        public double Latitude { get { return latitude; } }
        public double Longitude { get { return longitude; } }

        public int BatteryLevel { get { return battery_level; } }
        public int Status { get { return status; } }

        public bool HasBatteryService { get { return batteryService != null; } }

        public BackgroundTaskRegistration SmartPackDataTaskRegistration { get; set; }

        // settable properties, persisted in LocalSettings

        public bool AlertOnPhone
        {
            get { return alertOnPhone; }
            set
            {
                alertOnPhone = value;
                SaveSettings();
            }
        }
        public bool AlertOnDevice
        {
            get { return alertOnDevice; }
            set
            {
                alertOnDevice = value;
                SaveSettings();
            }
        }
        public AlertLevel AlertLevel
        {
            get { return alertLevel; }
            set
            {
                alertLevel = value;
                SaveSettings();
            }
        }

        public SmartPack(BluetoothLEDevice device)
        {
            this.device = device;
            addressString = device.BluetoothAddress.ToString("x012");

            battery_level = -1;
            status = -1;

            try
            {
                linkLossService = device.GetGattService(GattServiceUuids.LinkLoss);
            }
            catch (Exception)
            {
                // e.HResult == 0x80070490 means that the device doesn't have the requested service.
                // We can still alert on the phone upon disconnection, but cannot ask the device to alert.
                // linkLossServer will remain equal to null.
            }

            try
            {
                batteryService = device.GetGattService(GattServiceUuids.Battery);                
                heartRateService = device.GetGattService(GattServiceUuids.HeartRate);
            }
            catch (Exception)
            {

            }

            if (localSettings.Values.ContainsKey(addressString))
            {
                string[] values = ((string)localSettings.Values[addressString]).Split(',');
                alertOnPhone = bool.Parse(values[0]);
                alertOnDevice = bool.Parse(values[1]);
                alertLevel = (AlertLevel)Enum.Parse(typeof(AlertLevel), values[2]);
            }
            
            if (localSettings.Values.ContainsKey("backpack-location-latitude")
                && localSettings.Values.ContainsKey("backpack-location-longitude"))
            {
                latitude = (double)localSettings.Values["backpack-location-latitude"];
                longitude = (double)localSettings.Values["backpack-location-longitude"];
            }

        }

        public SmartPack(DeviceInformation device_max, BluetoothLEDevice device)
        {
            this.device = device;
            device_container_id = device_max.Properties["System.Devices.ContainerId"].ToString();
            device_id = device_max.Id;

            addressString = device.BluetoothAddress.ToString("x012");

            battery_level = -1;
            status = -1;

            try
            {
                linkLossService = device.GetGattService(GattServiceUuids.LinkLoss);
            }
            catch (Exception)
            {
                // e.HResult == 0x80070490 means that the device doesn't have the requested service.
                // We can still alert on the phone upon disconnection, but cannot ask the device to alert.
                // linkLossServer will remain equal to null.
            }


            try
            {
                batteryService = device.GetGattService(GattServiceUuids.Battery);
                heartRateService = device.GetGattService(GattServiceUuids.HeartRate);
            }
            catch (Exception)
            {

            }


            if (localSettings.Values.ContainsKey(addressString))
            {
                string[] values = ((string)localSettings.Values[addressString]).Split(',');
                alertOnPhone = bool.Parse(values[0]);
                alertOnDevice = bool.Parse(values[1]);
                alertLevel = (AlertLevel)Enum.Parse(typeof(AlertLevel), values[2]);
            }


            if (localSettings.Values.ContainsKey("backpack-location-latitude")
                && localSettings.Values.ContainsKey("backpack-location-longitude"))
            {
                latitude = (double)localSettings.Values["backpack-location-latitude"];
                longitude = (double)localSettings.Values["backpack-location-longitude"];
            }


        }

        private async Task<BluetoothLEDevice> ZZ(DeviceInformation device)
        {
            BluetoothLEDevice a = await BluetoothLEDevice.FromIdAsync(device.Id);
            return a;
        }

        // React to a change in configuration parameters:
        //    Save new values to local settings
        //    Set link-loss alert level on the device if appropriate
        //    Register or unregister background task if necessary
        private async void SaveSettings()
        {
            // Save this device's settings into nonvolatile storage
            string tmp = string.Join(",", alertOnPhone, alertOnDevice, alertLevel);
            localSettings.Values[addressString] = tmp;

            // If the device is connected and wants to hear about the alert level on link loss, tell it
            if (alertOnDevice && device.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                await SetAlertLevelCharacteristic();
            }

            // If we need a background task and one isn't already registered, create one
            if (TaskRegistration == null && (alertOnPhone || alertOnDevice))
            {
                DeviceConnectionChangeTrigger trigger = await DeviceConnectionChangeTrigger.FromIdAsync(device.DeviceId);
                trigger.MaintainConnection = true;
                BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
                builder.Name = TaskName;
                builder.TaskEntryPoint = "SmartPakkuBackground.ConnectivityTask";
                builder.SetTrigger(trigger);
                TaskRegistration = builder.Register();
            }

            // If we don't need a background task but have one, unregister it
            if (TaskRegistration != null && !alertOnPhone && !alertOnDevice)
            {
                TaskRegistration.Unregister(false);
                TaskRegistration = null;
            }

            // Can't forget about the battery!

            // If we need a battery_background task and one isn't already registered, create one
            if (SmartPackDataTaskRegistration == null && batteryService != null)
            {
                try
                {
                    GattCharacteristic BatteryLevelCharacteristic =   
                        batteryService.GetCharacteristics(GattCharacteristicUuids.BatteryLevel)[0];
                    GattCharacteristicNotificationTrigger bat_notify_trigger =
                        new GattCharacteristicNotificationTrigger(BatteryLevelCharacteristic);

                    BackgroundTaskBuilder battery_builder = new BackgroundTaskBuilder();
                    battery_builder.Name = TaskName + "2";
                    battery_builder.TaskEntryPoint = "SmartPakkuBackground.SmartPackDataTask";
                    battery_builder.SetTrigger(bat_notify_trigger);
                    SmartPackDataTaskRegistration = battery_builder.Register();
                }
                catch
                {

                }
            }

            // If we don't need a background task but have one, unregister it
            if (SmartPackDataTaskRegistration != null)
            {
                SmartPackDataTaskRegistration.Unregister(false);
                SmartPackDataTaskRegistration = null;
            }

        }

        // Set the alert-level characteristic on the remote device
        public async Task SetAlertLevelCharacteristic()
        {
            // try-catch block protects us from the race where the device disconnects
            // just after we've determined that it is connected.
            try
            {
                byte[] data = new byte[1];
                data[0] = (byte)alertLevel;

                // The LinkLoss service should contain exactly one instance of the AlertLevel characteristic
                GattCharacteristic characteristic = 
                    linkLossService.GetCharacteristics(GattCharacteristicUuids.AlertLevel)[0];
                await characteristic.WriteValueAsync(data.AsBuffer(), GattWriteOption.WriteWithResponse);
            }
            catch (Exception)
            {
                // ignore exception
            }
        }


        public async Task update_battery_level()
        {
            GattCharacteristic BatteryLevelCharacteristic = 
                batteryService.GetCharacteristics(GattCharacteristicUuids.BatteryLevel)[0];
            var result = await BatteryLevelCharacteristic.ReadValueAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                IBuffer tmp = result.Value;
                byte[] GattCharVals = tmp.ToArray();
                byte winner = GattCharVals[0];                
                battery_level = winner;
            }
            else
            {
                // cannot update
                battery_level = -1;
            }
        }

        public async Task update_status()
        {
            if (heartRateService != null)
            {
                GattCharacteristic HeartX =
                    heartRateService.GetCharacteristics(GattCharacteristicUuids.BodySensorLocation)[0];
                var result = await HeartX.ReadValueAsync();
                if (result.Status == GattCommunicationStatus.Success)
                {
                    IBuffer tmp = result.Value;
                    byte[] GattCharVals = tmp.ToArray();
                    byte winner = GattCharVals[0];
                    status = winner;
                }
                else
                {
                    // cannot update
                    status = -1;
                }
            }
        }

        // Provide a human-readable name for this object.
        public override string ToString()
        {
            return device.Name;
        }
    }

    public static class SmartPackHelper
    {
        public static void store_data(int what_to_store, int where_to_store_it)
        {

        }
    }
}