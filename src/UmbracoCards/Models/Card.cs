using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoCards.Models
{
    public class Card
    {
        [JsonProperty("link")]
        public string link { get; set; }

        [JsonProperty("content")]
        public string content { get; set; }

        [JsonProperty("thumb")]
        public string thumb { get; set; }

        [JsonProperty("publishedDate")]
        public string publishedDate { get; set; }
    }

    public class CardResponseMessage
    {
        public CardFeedData responseData { get; set; }
    }

    public class CardFeedData
    {
        [JsonProperty("feed")]
        public CardFeed Feed { get; set; }
    }

    public class CardFeed
    {
        [JsonProperty("entries")]
        public ICollection<Card> Entries { get; set; }
    }
}



