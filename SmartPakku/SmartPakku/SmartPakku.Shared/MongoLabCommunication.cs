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
                "\"fsrForces0\":{6},\"fsrForces1\":{7},\"fsrForces2\":{8},\"fsrForces3\":{9}}}",
                a_id,
                status,
                fsrPressed0.ToString().ToLower(), fsrPressed1.ToString().ToLower(), fsrPressed2.ToString().ToLower(), fsrPressed3.ToString().ToLower(),
                fsrForces0, fsrForces1, fsrForces2, fsrForces3
                );

            return WeightMeasurement_String;
        }
    }

    public class MongoLabCommunication
    {
        public static string MONGOLAB_API_KEY = "b6MjZVfMLjMkaYEWGiOgkqAOYshLnzMg";

        public async void DoSomething()
        {
            await GetMongo1();
            return;
        }

        public static async Task<dynamic> GetMongo1()
        {
            var client = new HttpClient();
            string reqUri = "https://api.mongolab.com/api/1/databases/smartpakku/collections/state?apiKey=b6MjZVfMLjMkaYEWGiOgkqAOYshLnzMg";
            var uri = new Uri(reqUri);
            var jstring = await client.GetStringAsync(uri);
            JsonValue jsonList = JsonValue.Parse(jstring);
            return jsonList;
        }

        public static async Task<dynamic> SendMongo1(string data)
        {
            var client = new HttpClient();
            var stuff = new HttpStringContent(data);
            stuff.Headers.ContentType= new HttpMediaTypeHeaderValue("application/json");

            string reqUri = string.Format("https://api.mongolab.com/api/1/databases/smartpakku/collections/state?apiKey={0}", MongoLabCommunication.MONGOLAB_API_KEY);
            var uri = new Uri(reqUri);

            var response = await client.PostAsync(uri, stuff);      
            return response;
        }
    }
}
