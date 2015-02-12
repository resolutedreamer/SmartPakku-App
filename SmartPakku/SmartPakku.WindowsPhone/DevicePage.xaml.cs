using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

//using KeyFob = KeepTheKeysCommon.KeyFob;

using SmartPakku.Common;
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
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using SmartPakkuCommon;

namespace SmartPakku
{
    public sealed partial class DevicePage : Page
    {

        ApplicationDataContainer my_settings = ApplicationData.Current.LocalSettings;


        public DevicePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = e.Parameter;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SmartPack SelectedSmartPack = (SmartPack)DataContext;

            //// WHAT DID I DO HERE?
            string SelectedSmartPackDeviceID = SelectedSmartPack.DeviceId;
            string x = (string)my_settings.Values[SelectedSmartPack.AddressString];
            /// NO REALLY I DON'T KNOW

            SelectedSmartPack.AlertOnPhone = (bool)alertOnPhoneCheckBox.IsChecked;

            var ToggleSwitchValue = GPSSwitch.IsOn;
            my_settings.Values["location-consent"] = ToggleSwitchValue;
            my_settings.Values["setup-wizard-complete"] = true;
            Frame.Navigate(typeof(MainPage), "no-refunds");
        }

        private void GPSSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var ToggleSwitchValue = GPSSwitch.IsOn;
            my_settings.Values["location-consent"] = ToggleSwitchValue;
        }
    }
}
