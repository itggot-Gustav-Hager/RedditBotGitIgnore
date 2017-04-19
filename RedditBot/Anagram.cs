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
            var splitAndCutSentence = sentence.Remove(0, 12).Split(' ');
            var correctedSentence = "";
            int i = 0;
            foreach (string word in splitAndCutSentence)
            {
                if(i <= 3)
                {
                    correctedSentence += $"{word} ";
                }
                i++;
            }
            



            var formData = new Dictionary<string, string>
            {
                    { "sourcetext", correctedSentence},
                    { "lang", "Swedish" },
                    { "lines", "1" },
                    { "maxword", "2" },
                    { "minchars", "1" },
                    { "sort", "0" },
                    {"outlang", "sv" },

                };
            var correctedContent = new FormUrlEncodedContent(formData).ReadAsStringAsync().GetAwaiter().GetResult();
            string url = $"https://www.arrak.fi/cgi-bin/inline_ag.pl?{correctedContent}";
            url = url.Replace("+", "%20");

            Console.WriteLine(url);
            var response = _client.GetStringAsync(url).GetAwaiter().GetResult();
            Console.WriteLine(response);
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(response);
            var anagram = html.DocumentNode.SelectSingleNode("pre").InnerHtml;

            return anagram;
        }
    }
    
}
