using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UmbracoCards.Models.Twitter;
using UmbracoCards.Models;
using Newtonsoft.Json.Linq;

namespace UmbracoCards.Services
{
    public class TwitterService : ICardService
    {
        private string _consumerKey;
        private string _consumerSecret;

        private string _encodedString
        {
            get
            {
                return GenerateEncodedString(_consumerKey, _consumerSecret);
            }
        }
        private string _bearerToken
        {
            get
            {
                var oauthResponse = ObtainBearerToken(_encodedString);
                return oauthResponse.AccessToken;
            }
        }

        public TwitterService(string consumerKey, string consumerSecret)
        {
            if (string.IsNullOrEmpty(consumerKey))
                throw new ArgumentNullException("twitter:consumerKey", "Missing appSettings key and value");

            if (string.IsNullOrEmpty(consumerSecret))
                throw new ArgumentNullException("twitter:consumerSecret", "Missing appSettings key and value");

            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
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
        private OAuthResponse ObtainBearerToken(string encodedString)
        {
            Uri uri = new Uri("https://api.twitter.com/oauth2/token");
            var reqparams = new NameValueCollection();
            reqparams.Add("grant_type", "client_credentials");

            var token = new OAuthResponse();
            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers[HttpRequestHeader.Authorization] = "Basic " + encodedString;
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                    byte[] responseBytes = client.UploadValues(uri, "POST", reqparams);
                    string response = Encoding.UTF8.GetString(responseBytes);
                    token = JsonConvert.DeserializeObject<OAuthResponse>(response);

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

            string queryStrings = string.Join("&", values.AllKeys.Select(p => p + "=" + values[p]).ToList());
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

        public ICollection<Card> ParseToCards(string response)
        {
            var data = JArray.Parse(response);
            var cards = new List<Card>();

            foreach (JObject entry in data)
            {
                string linkFormat = "https://twitter.com/{0}/status/{1}";
                string link = string.Format(linkFormat, entry["user"].Value<string>("name"), entry.Value<string>("id"));

                string thumb = "";
                if (entry.Value<JArray>("media").Any())
                {
                    var media = entry.Value<JArray>("media");
                    thumb = media[0].Value<string>("media_url");
                }

                var card = new Card()
                {
                    publishedDate = entry.Value<string>("created_at"),
                    content = entry.Value<string>("text"),
                    link = link,
                    thumb = thumb
                };

                cards.Add(card);
            }

            return cards;
        }


        #region helpers
        private string Encode(string data)
        {
            return Uri.EscapeDataString(data);
        }
        #endregion
    }


}