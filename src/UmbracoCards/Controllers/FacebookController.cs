using System.Collections.Specialized;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using UmbracoCards.Models;
using UmbracoCards.Services;

namespace SkolelederForeningen.Web.Controllers
{
    [PluginController("Cards")]
    public class FacebookController : UmbracoApiController
    {
        private string _appId = "";
        private string _appSecret = "";

        private string _accessToken
        {
            get { return string.Format("{0}|{1}", _appId, _appSecret); }
        }

        public FacebookController()
        {
            _appId = System.Configuration.ConfigurationManager.AppSettings["facebook:appId"] as string;
            _appSecret = System.Configuration.ConfigurationManager.AppSettings["facebook:appSecret"] as string;
        }

        public CardResponseMessage Get(string id, int limit = 20, string feed = "feed")
        {
            if (string.IsNullOrEmpty(_appId))
                throw new System.ArgumentNullException("facebook:appId", "Missing appSettings key and value!");

            if (string.IsNullOrEmpty(_appSecret))
                throw new System.ArgumentNullException("facebook:appSecret", "Missing appSetting key and value!");

            var values = new NameValueCollection();
            values.Add("key", "value");
            values.Add("access_token", _accessToken);
            string fields = "id,message,picture,link,name,description,type,icon,created_time,from,object_id,likes,comments";
            values.Add("fields", fields);

            string urlFormat = "https://graph.facebook.com/v2.5/{0}/{1}?key=value&access_token={2}&fields={3}";
            string url = string.Format(urlFormat, id, feed, _accessToken, fields);

            var service = new FacebookService(_appId, _appSecret);
            string response = service.Get(url, values);
            var cards = service.ParseToCards(response);

            var message = new CardResponseMessage
            {
                responseData = new CardFeedData
                {
                    Feed = new CardFeed
                    {
                        Entries = cards
                    }
                }
            };

            return message;
        }
    }
}