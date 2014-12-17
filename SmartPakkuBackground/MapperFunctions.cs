using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Maps;

namespace SmartPakkuBackground
{
    public sealed class MapperFunctions
    {
        ApplicationDataContainer my_settings = ApplicationData.Current.LocalSettings;

        private Geolocator locator;
        //private CoreDispatcher _cd;

        private Geoposition my_position;
        private Geopoint my_point;
        private MapIcon my_icon;
        private MapIcon my_pack_icon;

        

        public MapIcon get_icon()
        {
            my_icon = new MapIcon();
            if (my_point != null)
            {                
                my_icon.Location = my_point;
                my_icon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                my_icon.Title = "My Location";
                return my_icon;
            }
            else
            {
                return null;
            }
        }

        public MapIcon get_backpack_icon()
        {
            my_pack_icon = new MapIcon();
            if (my_settings.Values.ContainsKey("backpack-location-latitude") && my_settings.Values.ContainsKey("backpack-location-longitude"))
            {
                BasicGeoposition tmp_position = new BasicGeoposition();
                tmp_position.Latitude = (double)my_settings.Values["backpack-location-latitude"];
                tmp_position.Longitude = (double)my_settings.Values["backpack-location-longitude"];
                Geopoint tmp_point = new Geopoint(tmp_position);

                my_pack_icon.Location = tmp_point;
                my_pack_icon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                my_pack_icon.Title = "Backpack Location";
                return my_pack_icon;
            }
            else
            {
                return null;
            }
        }

        public MapperFunctions()
        {
            locator = new Geolocator();
            locator.DesiredAccuracyInMeters = 50;
        }

        public async Geoposition get_current_position()
        {
            await update_current_location();
            return my_position;
        }

        public async Geopoint get_current_point()
        {
            await update_current_location();
            return my_point;
        }

        private async Task update_current_location()
        {
            my_position = await locator.GetGeopositionAsync();
            my_point = my_position.Coordinate.Point;
        }



        public void store_current_location()
        {
            my_settings.Values["backpack-location-latitude"] = my_position.Coordinate.Point.Position.Latitude;
            my_settings.Values["backpack-location-longitude"] = my_position.Coordinate.Point.Position.Longitude;
        }


        public Geopoint get_saved_backpack_location()
        {
            if (my_settings.Values.ContainsKey("backpack-location-latitude") && my_settings.Values.ContainsKey("backpack-location-longitude"))
            {
                double lat = Convert.ToDouble(my_settings.Values["backpack-location-latitude"].ToString());
                double lon = Convert.ToDouble(my_settings.Values["backpack-location-longitude"].ToString());

                BasicGeoposition myPosition = new BasicGeoposition();
                myPosition.Latitude = lat;
                myPosition.Longitude = lon;

                Geopoint myPoint = new Geopoint(myPosition);
                return myPoint;
            }
            else
            {
                return null;
            }
        }
    }
}
