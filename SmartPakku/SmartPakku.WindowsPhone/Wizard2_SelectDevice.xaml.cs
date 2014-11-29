using SmartPakku.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
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




// I Added This Section

using System.Collections.ObjectModel;
using SmartPakkuCommon;

//  Make it obvious which namespace provided each referenced type:
using BackgroundAccessStatus = Windows.ApplicationModel.Background.BackgroundAccessStatus;
using BackgroundExecutiondManager = Windows.ApplicationModel.Background.BackgroundExecutionManager;
using BackgroundTaskRegistration = Windows.ApplicationModel.Background.BackgroundTaskRegistration;
using DeviceInformation = Windows.Devices.Enumeration.DeviceInformation;
using BluetoothLEDevice = Windows.Devices.Bluetooth.BluetoothLEDevice;

// End Section


namespace SmartPakku
{
    public sealed partial class Wizard2_SelectDevice : Page
    {

        

        public ObservableCollection<SmartPack> Devices { get; private set; }

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();


        public Wizard2_SelectDevice()
        {

            Devices = new ObservableCollection<SmartPack>();


            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }


        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }
        #region NavigationHelper registration

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {

            // Request the right to have background tasks run in the future. This need only be done once
            // after the app is installed, but it is harmless to do it every time the app is launched.
            if (await BackgroundExecutiondManager.RequestAccessAsync() == BackgroundAccessStatus.Denied)
            {
                // TODO: What?
            }

            // Acquire the set of background tasks that we already have registered. Store them into a dictionary, keyed
            // by task name. (For each LE device, we will use a task name that is derived from its Bluetooth address).
            Dictionary<string, BackgroundTaskRegistration> taskRegistrations = new Dictionary<string, BackgroundTaskRegistration>();
            foreach (BackgroundTaskRegistration reg in BackgroundTaskRegistration.AllTasks.Values)
            {
                taskRegistrations[reg.Name] = reg;
            }


            // Get the list of paired Bluetooth LE devicdes, and add them to our 'devices' list. Associate each device with
            // its pre-existing registration if any, and remove that registration from our dictionary.
            Devices.Clear();
            foreach (DeviceInformation di in await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelector()))
            {
                BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromIdAsync(di.Id);
                SmartPack device = new SmartPack(bleDevice);
                if (taskRegistrations.ContainsKey(device.TaskName))
                {
                    device.TaskRegistration = taskRegistrations[device.TaskName];
                    taskRegistrations.Remove(device.TaskName);
                }
                Devices.Add(device);
            }
            // Unregister any remaining background tasks that remain in our dictionary. These are tasks that we registered
            // for Bluetooth LE devices that have since been unpaired.
            foreach (BackgroundTaskRegistration reg in taskRegistrations.Values)
            {
                reg.Unregister(false);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        private void deviceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (deviceListBox.SelectedItem != null)
            {
                Frame.Navigate(typeof(DevicePage), deviceListBox.SelectedItem);
            }
        }
    }
}
