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

///////////////////////////////////////////////////////////using KeyFob = KeepTheKeysCommon.KeyFob;



namespace SmartPakkuBackground
{
    public sealed class BatteryTask
    {
    }
}
