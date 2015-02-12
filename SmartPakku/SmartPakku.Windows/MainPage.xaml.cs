using Bing.Maps;
using SmartPakku.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;



// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace SmartPakku
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        ApplicationDataContainer my_settings = ApplicationData.Current.LocalSettings;
        MongoLabCredentials user_credentials;

        //Geoposition pos; private Geolocator locator = null; private CoreDispatcher _cd;
        //Geoposition my_position; Geopoint my_point; //MapIcon my_icon; MapIcon BackpackHere;
        
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public static MainPage Current { get; internal set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }


        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            user_credentials = (MongoLabCredentials)e.Parameter;
            JsonObject MyData = await MongoLabCommunication.refresh_data_json(user_credentials);
            refresh_text(MyData);
            refresh_map(MyData);
            navigationHelper.OnNavigatedTo(e);            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            JsonObject MyData = await MongoLabCommunication.refresh_data_json(user_credentials);
            refresh_text(MyData);
            refresh_map(MyData);
        }

        

        private void refresh_text(JsonObject MyData)
        {
            IJsonValue left_shoulder_data, left_waist_data, right_shoulder_data, right_waist_data;
            IJsonValue left_shoulder_weight_data, left_waist_weight_data, right_waist_weight_data, right_shoulder_weight_data;
            bool x;
            x = MyData.TryGetValue("fsrPressed0", out left_shoulder_data);
            x = MyData.TryGetValue("fsrForces0", out left_shoulder_weight_data);

            x = MyData.TryGetValue("fsrPressed1", out left_waist_data);
            x = MyData.TryGetValue("fsrForces1", out left_waist_weight_data);

            x = MyData.TryGetValue("fsrPressed2", out right_shoulder_data);
            x = MyData.TryGetValue("fsrForces2", out right_waist_weight_data);

            x = MyData.TryGetValue("fsrPressed3", out right_waist_data);
            x = MyData.TryGetValue("fsrForces3", out right_shoulder_weight_data);
            bool left_shoulder_value = left_shoulder_data.GetBoolean();
            double left_shoulder_weight_value = left_shoulder_weight_data.GetNumber();

            bool left_waist_value = left_waist_data.GetBoolean();
            double left_waist_weight_value = left_waist_weight_data.GetNumber();

            bool right_shoulder_value = right_shoulder_data.GetBoolean();
            double right_shoulder_weight_value = right_shoulder_weight_data.GetNumber();

            bool right_waist_value = right_waist_data.GetBoolean();
            double right_waist_weight_value = right_waist_weight_data.GetNumber();

            left_shoulder.Text = "Left Shoulder: " + left_shoulder_value.ToString();
            left_shoulder_weight.Text = "Weight: " + left_waist_weight_value.ToString();

            left_waist.Text = "Left Waist: " + left_waist_value.ToString();
            left_waist_weight.Text = "Weight: " + left_waist_weight_value.ToString();

            right_shoulder.Text = "Right Waist: " + right_shoulder_value.ToString();
            right_shoulder_weight.Text = "Weight: " + right_shoulder_weight_value.ToString();

            right_waist.Text = "Right Waist: " + right_waist_value.ToString();
            right_waist_weight.Text = "Weight: " + right_waist_weight_value.ToString();

            time_text.Text = "Last Updated: " + DateTime.Now;
        }

        private void refresh_map(JsonObject MyData)
        {
            IJsonValue LocationX_data, LocationY_data;
            bool x;
            x = MyData.TryGetValue("locationX", out LocationX_data);
            x = MyData.TryGetValue("locationY", out LocationY_data);
            double LocationX = LocationX_data.GetNumber();
            double LocationY = LocationY_data.GetNumber();

            myMap.SetView(new Location(LocationY, LocationX));
            //MapLayer.SetPosition(new Location(46.849947, -121.32168));
        }

        private async void pushpinTapped(object sender, TappedRoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("Hello from Seattle.");
            await dialog.ShowAsync();
        }
    }
}
