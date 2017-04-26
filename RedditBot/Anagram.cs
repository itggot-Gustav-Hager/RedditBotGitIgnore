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
        /// Only takes set amount of words
        /// Takes maximum of 20 characters
        /// </summary>
        /// <param name="sentence">String, Can't be longer than 8 words</param>
        /// <example>
        /// var anagram = anagramizer.Anagramize("hejsan");
        /// anagram == "hajens";
        /// </example>
        public string Anagramize(string sentence)
        {
            var splitAndCutSentence = sentence.Remove(0, 12).Split(' ');
            var correctedSentence = "";
            int i = 0;
            foreach (string word in splitAndCutSentence)
            {
                if(i <= 3 && correctedSentence.Length <= 20)
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
                    { "maxword", "8" },
                    { "minchars", "1" },
                    { "sort", "0" },
                    {"outlang", "sv" },

                };
            var correctedContent = new FormUrlEncodedContent(formData).ReadAsStringAsync().GetAwaiter().GetResult();
            string url = $"https://www.arrak.fi/cgi-bin/inline_ag.pl?{correctedContent}";
            url = url.Replace("+", "%20");

            var response = _client.GetStringAsync(url).GetAwaiter().GetResult();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(response);
            var anagram = html.DocumentNode.SelectSingleNode("pre").InnerHtml;
            if(anagram == "\n\t\t")
            {
                anagram = "Could not create an anagram, use other characters, no more than two words, more characters or fewer characters (not more than 20, not less than 1)";
            }
            return anagram;
        }
    }
    
}
