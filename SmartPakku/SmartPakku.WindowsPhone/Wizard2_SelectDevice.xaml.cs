using SmartPakku.Common;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// I Added This Section

using System.Collections.ObjectModel;
using SmartPakkuCommon;

//  Make it obvious which namespace provided each referenced type:
using DeviceInformation = Windows.Devices.Enumeration.DeviceInformation;
using BluetoothLEDevice = Windows.Devices.Bluetooth.BluetoothLEDevice;
using Windows.Storage;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

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
            foreach (DeviceInformation di in await DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.HeartRate), new string[] { "System.Devices.ContainerId" }))
            //BluetoothLEDevice.GetDeviceSelector(), new string[] { "System.Devices.ContainerId" } ))
            {
                string Selected_Device_ID = di.Id;
                BluetoothLEDevice bleDevice = await BluetoothLEDevice.FromIdAsync(Selected_Device_ID);                
                SmartPack device = new SmartPack(di, bleDevice);
                Devices.Add(device);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        private void deviceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            // We selected SmartPack x, so save the DeviceId of this SmartPack into settings
            var SelectedSmartPack = (SmartPack)deviceListBox.SelectedItem;
            localSettings.Values["smartpack-device-id"] = SelectedSmartPack.DeviceId;

            if (deviceListBox.SelectedItem != null)
            {
                Frame.Navigate(typeof(DevicePage), SelectedSmartPack);
            }
        }

        private void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Wizard3_MongoKeys));
        }
    }
}
