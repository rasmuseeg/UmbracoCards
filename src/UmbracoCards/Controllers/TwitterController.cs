using Newtonsoft.Json;
using SocialStream.Models.Twitter;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace SkolelederForeningen.Web.Controllers
{
    [PluginController("Cards")]
    public class TwitterController : UmbracoApiController
    {
        private TwitterService _auth;
        private string _consumerKey = "";
        private string _consumerSecret = "";

        public TwitterController()
        {
            _consumerKey = System.Configuration.ConfigurationManager.AppSettings["twitter:consumerKey"] as string;
            _consumerSecret = System.Configuration.ConfigurationManager.AppSettings["twitter:consumerSecret"] as string;
            _auth = new TwitterService(_consumerKey, _consumerSecret);
        }

        // ?url=timeline&screen_name=jquery&count=3&include_entities=true&include_rts=false&exclude_replies=true&url=search&q=designchemical
        public string Get(string url = "", string screen_name = "", string include_entities = "", string include_rts = "", string exclude_replies = "", string list_id = "", int count = 20, string q = "")
        {
            string rest = "";
            var queryStrings = new NameValueCollection();
            switch (url)
            {
                case "timeline":
                    rest = "statuses/user_timeline";
                    queryStrings.Add("count", count.ToString());
                    queryStrings.Add("include_rts", include_rts);
                    queryStrings.Add("exclude_replies", exclude_replies);
                    queryStrings.Add("screen_name", screen_name);
                    break;
                case "search":
                    rest = "search/tweets";
                    queryStrings.Add("q", q);
                    queryStrings.Add("count", count.ToString());
                    queryStrings.Add("include_rts", include_rts);
                    break;
                case "list":
                    rest = "lists/statuses";
                    queryStrings.Add("list_id", list_id);
                    queryStrings.Add("count", count.ToString());
                    queryStrings.Add("include_rts", include_rts);
                    break;
                default:
                    rest = "statuses/user_timeline";
                    queryStrings.Add("count", count.ToString());
                    break;
            }
            
            string response = _auth.Get(rest, queryStrings);
            return response;
        }
    }
}

namespace SocialStream.Models.Twitter
{
    public class TwitterService
    {
        private string _consumerKey;
        private string _consumerSecret;

        private string _bearerToken;
        private string _accessTokenSecret;

        public TwitterService(string consumerKey, string consumerSecret)
        {
            if (string.IsNullOrEmpty(consumerKey))
                throw new ArgumentNullException( "twitter:consumerKey", "Missing appSettings key and value");

            if(string.IsNullOrEmpty(consumerSecret))
                throw new ArgumentNullException("twitter:consumerSecret", "Missing appSettings key and value");

            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;

            var encodedString = GenerateEncodedString(_consumerKey, _consumerSecret);
            TwitterToken token = ObtainBearerToken(encodedString);
            _bearerToken = token.AccessToken;
        }

        #region OAuth
        // Step 1: Encode consumer key and secret
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        private string GenerateEncodedString(string consumerKey, string consumerSecret)
        {
            // Step 1.1
            // URL encode the consumer key and the consumer secret according to RFC 1738. 
            // Note that at the time of writing, this will not actually change the consumer key and secret, 
            // but this step should still be performed in case the format of those values changes in the future.
            string encodedConsumerKey = Encode(consumerKey);
            string encodedConsumarSecret = Encode(consumerSecret);

            // Step 1.2
            // Concatenate the encoded consumer key, a colon character “:”, and the encoded consumer secret into a single string.
            string encodedValues = string.Format("{0}:{1}", encodedConsumerKey, encodedConsumarSecret);

            // Step 1.3
            // Base64 encode the string from the previous step.
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(encodedValues);
            string encodedString = System.Convert.ToBase64String(plainTextBytes);

            return encodedString;
        }

        // Step 2: Obtain a bearer token
        private TwitterToken ObtainBearerToken(string encodedString)
        {
            Uri uri = new Uri("https://api.twitter.com/oauth2/token");
            var reqparams = new NameValueCollection();
            reqparams.Add("grant_type", "client_credentials");

            var token = new TwitterToken();
            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers[HttpRequestHeader.Authorization] = "Basic " + encodedString;
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    
                    byte[] responseBytes = client.UploadValues(uri, "POST", reqparams);
                    string response = Encoding.UTF8.GetString(responseBytes);
                    token = JsonConvert.DeserializeObject<TwitterToken>(response);

                    if (token.Type == "bearer")
                        return token;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return token;
        }

        /// <summary>
        /// Should a bearer token become compromised or need to be invalidated for any reason, issue a call to POST oauth2 / invalidate_token.
        /// </summary>
        private void InvalidateToken()
        {
            using (var client = new WebClient())
            {
            }
        }
        #endregion

        // Step 3: Authenticate API requests with the bearer token
        // The bearer token may be used to issue requests to API endpoints 
        // which support application-only auth. To use the bearer token, 
        // construct a normal HTTPS request and include an Authorization 
        // header with the value of Bearer <base64 bearer token value from step 2>. 
        // Signing is not required.
        /// <summary>
        /// Performs a REST request using the bearer token
        /// </summary>
        /// <param name="url">Url to download from, can also be alias. Like so: statuses/user_timeline</param>
        /// <param name="values">Querystrings to append</param>
        /// <returns>responseData</returns>
        public string Get(string url, NameValueCollection values)
        {
            string rest = "https://api.twitter.com/1.1/";

            if (!url.Contains("//"))
                rest += url + ".json";
            else
                rest = url;

            string queryStrings = string.Join("&", values.AllKeys.Select(p=>p +"="+ values[p]).ToList());
            rest += "?" + queryStrings;

            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + _bearerToken;
                    return client.DownloadString(rest);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        

        #region helpers
        private string Encode(string data)
        {
            return Uri.EscapeDataString(data);
        }
        #endregion
    }

    public class TwitterToken
    {
        [JsonProperty("token_type")]
        public string Type { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}