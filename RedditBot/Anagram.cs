using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using HtmlAgilityPack;

namespace RedditBot
{
    class Anagram
    {
        private HttpClient _client;

        public Anagram()
        {
            _client = new HttpClient();
        }
        /// <summary>
        /// Creates an anagram of a given sentence
        /// </summary>
        /// <param name="sentence">String, Can't be longer than 8 words</param>
        public string Anagramize(string sentence)
        {
            var correctedSentence = sentence.Remove(0, 12);

            var formData = new Dictionary<string, string>
            {
                    { "sourcetext", correctedSentence },
                    { "lang", "Swedish" },
                    { "lines", "1" },
                    { "maxword", "8" }
                };
            var correctedContent = new FormUrlEncodedContent(formData).ReadAsStringAsync().GetAwaiter().GetResult();
            var url = $"https://www.arrak.fi/cgi-bin/inline_ag.pl?{correctedContent}";
            var response = _client.GetStringAsync(url).GetAwaiter().GetResult();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(response);
            var anagram = html.DocumentNode.SelectSingleNode("pre").InnerHtml;

            return anagram;
        }
    }
    
}
