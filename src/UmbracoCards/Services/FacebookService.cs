using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UmbracoCards.Models;

namespace UmbracoCards.Services
{
    public class FacebookService : ICardService
    {
        private string _appId;
        private string _appSecret;

        private string _accessToken
        {
            get { return string.Format("{0}|{1}", _appId, _appSecret); }
        }

        public FacebookService(string appId, string appSecret)
        {
            if (string.IsNullOrEmpty(appId))
                throw new System.ArgumentNullException("facebook:appId", "Missing appSettings key and value!");

            if (string.IsNullOrEmpty(appSecret))
                throw new System.ArgumentNullException("facebook:appSecret", "Missing appSetting key and value!");

            _appId = appId;
            _appSecret = appSecret;
        }

        public string Get(string url, NameValueCollection values)
        {
            string queryStrings = string.Join("&", values.AllKeys.Select(p => string.Format("{0}={1}", p, values[p])).ToList());

            try
            {
                using (WebClient client = new WebClient())
                {
                    return client.DownloadString(url);
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        public ICollection<Card> ParseToCards(string response)
        {
            var data = JObject.Parse(response);
            var cards = new List<Card>();

            foreach (var item in data["data"])
            {
                string message = "";
                // Message
                if (!string.IsNullOrEmpty(item.Value<string>("message")))
                {
                    message = item.Value<string>("message");
                }
                else if (!string.IsNullOrEmpty(item.Value<string>("story")))
                {
                    message = item.Value<string>("story");
                }

                // Description
                if (!string.IsNullOrEmpty(item.Value<string>("description")))
                {
                    message += " " + item.Value<string>("description");
                }

                string link = !string.IsNullOrEmpty(item.Value<string>("link")) ? item.Value<string>("link") : "";
                string image = !string.IsNullOrEmpty(item.Value<string>("picture")) ? item.Value<string>("picture") : null;
                string type = !string.IsNullOrEmpty(item.Value<string>("type")) ? item.Value<string>("type") : "";

                if (string.IsNullOrEmpty(link))
                    continue;

                cards.Add(new Card
                {
                    link = link,
                    content = message,
                    thumb = image,
                    publishedDate = item.Value<string>("created_time")
                });
            }

            return cards;
        }
    }
}
