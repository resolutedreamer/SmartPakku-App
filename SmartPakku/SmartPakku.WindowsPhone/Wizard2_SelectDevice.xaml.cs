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
            // Get the list of paired Bluetooth LE devicdes, and add them to our 'devices' list. Associate each device with
            // its pre-existing registration if any, and remove that registration from our dictionary.
            Devices.Clear();
            foreach (DeviceInformation di in await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelector()))
            {
                BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromIdAsync(di.Id);
                SmartPack device = new SmartPack(bleDevice);

                Devices.Add(device);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion



        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the list of paired Bluetooth LE devicdes, and add them to our 'devices' list. Associate each device with
            // its pre-existing registration if any, and remove that registration from our dictionary.
            Devices.Clear();
            foreach (DeviceInformation di in await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelector()))
            {
                BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromIdAsync(di.Id);
                SmartPack device = new SmartPack(bleDevice);
                Devices.Add(device);
            }
        }

        private void deviceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (deviceListBox.SelectedItem != null)
            {
                Frame.Navigate(typeof(DevicePage), deviceListBox.SelectedItem);
            }
        }
    }
}
