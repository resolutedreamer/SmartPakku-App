using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Data.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace SmartPakku
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WeightMeasurementTest : Page
    {
        public WeightMeasurementTest()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            WeightMeasurement another_measurement = new WeightMeasurement();
            another_measurement.state_ID = "54b726e9e4b064b9f80549ab";

            // we got an 's' last time so this time we got status
            another_measurement.status = (short)Convert.ToInt16(status.Text);

            var A = another_measurement.status & 1;
            var B = another_measurement.status & 2;
            var C = another_measurement.status & 4;
            var D = another_measurement.status & 8;

            // FSR0 Pressed
            // Left shoulder
            if (A == 1)
                another_measurement.fsrPressed0 = true;
            else
                another_measurement.fsrPressed0 = false;

            // FSR1 Pressed
            // Left Waist
            if (B == 2)
                another_measurement.fsrPressed1 = true;
            else
                another_measurement.fsrPressed1 = false;

            // FSR2 Pressed
            // Right Waist
            if (C == 4)
                another_measurement.fsrPressed2 = true;
            else
                another_measurement.fsrPressed2 = false;

            // FSR2 Pressed
            // Right shoulder
            if (D == 8)
                another_measurement.fsrPressed3 = true;
            else
                another_measurement.fsrPressed3 = false;
            another_measurement.fsrForces0 = (ushort)Convert.ToUInt16(fsrForces0.Text);
            another_measurement.fsrForces1 = (ushort)Convert.ToUInt16(fsrForces1.Text);
            another_measurement.fsrForces2 = (ushort)Convert.ToUInt16(fsrForces2.Text);
            another_measurement.fsrForces3 = (ushort)Convert.ToUInt16(fsrForces3.Text);

            string sendthis = another_measurement.ToString();

            

            var success = await MongoLabCommunication.SendMongo1(sendthis);
        }
    }
}
