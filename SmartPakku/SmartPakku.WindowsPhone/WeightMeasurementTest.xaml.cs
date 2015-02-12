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
    public sealed partial class WeightMeasurementTest : Page
    {
        public WeightMeasurementTest()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            send_fake_data();
        }
        public async void send_fake_data()
        {
            WeightMeasurement another_measurement = new WeightMeasurement();
            another_measurement.state_ID = state_id.Text;

            // status
            another_measurement.status = (short)Convert.ToInt16(status.Text);

            var A = another_measurement.status & 1;
            var B = another_measurement.status & 2;
            var C = another_measurement.status & 4;
            var D = another_measurement.status & 8;

            // FSR0, Left shoulder
            another_measurement.fsrPressed0 = (A == 1) ? true : false;
            another_measurement.fsrForces0 = (ushort)Convert.ToUInt16(fsrForces0.Text);

            // FSR1, Left Waist
            another_measurement.fsrPressed1 = (B == 2) ? true : false;
            another_measurement.fsrForces1 = (ushort)Convert.ToUInt16(fsrForces1.Text);

            // FSR2, Right Waist
            another_measurement.fsrPressed2 = (C == 4) ? true : false;
            another_measurement.fsrForces2 = (ushort)Convert.ToUInt16(fsrForces2.Text);

            // FSR2, Right shoulder
            another_measurement.fsrPressed3 = (D == 8) ? true : false;
            another_measurement.fsrForces3 = (ushort)Convert.ToUInt16(fsrForces3.Text);

            another_measurement.locationX = Convert.ToDouble(LocationX.Text);
            another_measurement.locationY = Convert.ToDouble(LocationY.Text);

            string sendthis = another_measurement.ToString();

            var success = await MongoLabCommunication.SendMongo1(sendthis, MongoLabCommunication.default_credentials);
        }

    }
}
