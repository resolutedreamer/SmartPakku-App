using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
using Windows.ApplicationModel.Background;
using SmartPakkuCommon;
using Windows.Devices.Bluetooth.Background;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

///////////////////////////////////////////////////////////using KeyFob = KeepTheKeysCommon.KeyFob;


namespace SmartPakkuBackground
{
    public sealed class BatteryTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            try
            {
                var details = (GattCharacteristicNotificationTriggerDetails)taskInstance.TriggerDetails;
                GattCharacteristic tmp_characteristic = details.Characteristic;
                if (tmp_characteristic.Uuid == GattCharacteristicUuids.BatteryLevel)
                {
                    // the battery level indeed was the notifier that triggered this background task
                    // we can work on it immediately
                }
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
