//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
//
//*********************************************************

using System.Collections.Generic;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using SmartPakku;


using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;

namespace SmartPakku
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scenario1_DeviceEvents : Page
    {
        public Scenario1_DeviceEvents()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached. The Parameter property is typically
        /// used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (HeartRateService.Instance.IsServiceInitialized)
            {
                foreach (var measurement in HeartRateService.Instance.DataPoints)
                {
                    outputListBox_page2.Items.Add(measurement.ToString());
                }
                outputGrid.Visibility = Visibility.Visible;
                RunButton.IsEnabled = false;
            }
            HeartRateService.Instance.ValueChangeCompleted += Instance_ValueChangeCompleted;
        }
        
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HeartRateService.Instance.ValueChangeCompleted -= Instance_ValueChangeCompleted;
        }

        private async void Instance_ValueChangeCompleted(HeartRateMeasurement heartRateMeasurementValue)
        {
            // Serialize UI update to the the main UI thread.
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                statusTextBlock.Text = "Latest received heart rate measurement: " +
                    heartRateMeasurementValue.HeartRateValue;

                outputListBox_page2.Items.Insert(0, heartRateMeasurementValue);
            });
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;

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
                statusTextBlock.Text = "Could not find any Heart Rate devices. Please make sure your device is paired " +
                    "and powered on!";
            }
            RunButton.IsEnabled = true;
        }

        private async void DevicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            RunButton.IsEnabled = false;
            var device = DevicesListBox.SelectedItem as DeviceInformation;
            DevicesListBox.Visibility = Visibility.Collapsed;

            statusTextBlock.Text = "Initializing device...";
            HeartRateService.Instance.DeviceConnectionUpdated += OnDeviceConnectionUpdated;
            await HeartRateService.Instance.InitializeServiceAsync(device);

            outputGrid.Visibility = Visibility.Visible;

            try
            {
                // Check if the device is initially connected, and display the appropriate message to the user
                var deviceObject = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer,
                    device.Properties["System.Devices.ContainerId"].ToString(), 
                    new string[] { "System.Devices.Connected" });
            
                bool isConnected;
                if (Boolean.TryParse(deviceObject.Properties["System.Devices.Connected"].ToString(), out isConnected))
                {
                    OnDeviceConnectionUpdated(isConnected);
                }
            } 
            catch (Exception)
            {
                statusTextBlock.Text = "Retrieving device properties failed";
            }
        }

        private async void OnDeviceConnectionUpdated(bool isConnected)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (isConnected)
                {
                    statusTextBlock.Text = "Waiting for device to send data...";
                }
                else
                {
                    statusTextBlock.Text = "Waiting for device to connect...";
                }
            });
        }


    }
}
