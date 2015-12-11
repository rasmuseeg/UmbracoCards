using System.Collections.Generic;
using System.Collections.Specialized;
using UmbracoCards.Models;

namespace UmbracoCards.Services
{
    public interface ICardService
    {
        string Get(string url, NameValueCollection values);

        ICollection<Card> ParseToCards(string data);
    }
}