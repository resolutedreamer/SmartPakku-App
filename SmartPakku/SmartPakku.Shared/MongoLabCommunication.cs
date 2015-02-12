using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace SmartPakku
{
    public class WeightMeasurement
    {
        public short status { get; set; }
        public bool fsrPressed0 { get; set; }
        public bool fsrPressed1 { get; set; }
        public bool fsrPressed2 { get; set; }
        public bool fsrPressed3 { get; set; }

        public ushort fsrForces0 { get; set; }
        public ushort fsrForces1 { get; set; }
        public ushort fsrForces2 { get; set; }
        public ushort fsrForces3 { get; set; }
        public double locationX { get; set; }
        public double locationY { get; set; }

        public bool ready { get; set; }

        public string state_ID { get; set; }

        public override string ToString()
        {
            string abcd = state_ID;
            string a_id = string.Format("{{\"$oid\":\"{0}\"}}", abcd);

            string WeightMeasurement_String =
                string.Format(
                "{{\"_id\":{0},\"state\":{1}," +
                "\"fsrPressed0\":{2},\"fsrPressed1\":{3},\"fsrPressed2\":{4},\"fsrPressed3\":{5}," +
                "\"fsrForces0\":{6},\"fsrForces1\":{7},\"fsrForces2\":{8},\"fsrForces3\":{9},\"locationX\":{10},\"locationY\":{11}}}",
                a_id,
                status,
                fsrPressed0.ToString().ToLower(), fsrPressed1.ToString().ToLower(), fsrPressed2.ToString().ToLower(), fsrPressed3.ToString().ToLower(),
                fsrForces0, fsrForces1, fsrForces2, fsrForces3,
                locationX, locationY
                );

            return WeightMeasurement_String;
        }
    }

    public class MongoLabCredentials
    {
        public string API_KEY;
        public string STATE_ID;
    }

    public class MongoLabCommunication
    {
        public static MongoLabCredentials default_credentials = new
        MongoLabCredentials
        {
            API_KEY = "b6MjZVfMLjMkaYEWGiOgkqAOYshLnzMg",
            STATE_ID = "54b726e9e4b064b9f80549ab"
        };

        public static async Task<dynamic> SendMongo1(string data, MongoLabCredentials auth_credentials)
        {
            var client = new HttpClient();
            var stuff = new HttpStringContent(data);
            stuff.Headers.ContentType = new HttpMediaTypeHeaderValue("application/json");

            string reqUri = string.Format("https://api.mongolab.com/api/1/databases/smartpakku/collections/state?apiKey={0}", auth_credentials.API_KEY);
            var uri = new Uri(reqUri);

            var response = await client.PostAsync(uri, stuff);
            return response;
        }


        public static async Task<JsonObject> refresh_data_json(MongoLabCredentials user_credentials)
        {
            JsonObject MyData = new JsonObject();
            try
            {
                MyData = await MongoLabCommunication.GetMongo1(user_credentials);
                return MyData;
            }
            catch (Exception)
            {
            }
            return MyData;
        }

        public static async Task<dynamic> GetMongo1(MongoLabCredentials auth_credentials)
        {
            var client = new HttpClient();
            string reqUri = string.Format("https://api.mongolab.com/api/1/databases/smartpakku/collections/state?q={{\"_id\":{{\"$oid\":\"{0}\"}}}}&apiKey={1}",
                auth_credentials.STATE_ID, auth_credentials.API_KEY);

            var uri = new Uri(reqUri);
            var jstring = await client.GetStringAsync(uri);
            JsonValue jsonList = JsonValue.Parse(jstring);
            JsonObject jsonObject = jsonList.GetArray().GetObjectAt(0);
            return jsonObject;
        }


    }
}
