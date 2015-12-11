using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoCards.Models.Twitter
{
    public class OAuthResponse
    {
        [JsonProperty("token_type")]
        public string Type { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
