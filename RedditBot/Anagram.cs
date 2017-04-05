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
            byte[] bytes = new byte[correctedSentence.Length * sizeof(char)];
            Buffer.BlockCopy(correctedSentence.ToCharArray(), 0, bytes, 0, bytes.Length);
            Encoding utf8 = new UTF8Encoding();
            Encoding w1252 = Encoding.GetEncoding(1252);
            byte[] output = Encoding.Convert(utf8, w1252, bytes);
            correctedSentence = w1252.GetString(output);

            var formData = new Dictionary<string, string>
            
            {
                    { "sourcetext", correctedSentence },
                    { "lang", "Swedish" },
                    { "lines", "1000" },
                    { "maxword", "8" }
                };
            var url = $"https://www.arrak.fi/cgi-bin/inline_ag.pl?sourcetext={correctedSentence}&lang=Swedish&lines=1&maxword=8&minchars=1&sort=0&outlang=sv";
            var response = _client.GetStringAsync(url).GetAwaiter().GetResult();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(response);
            var anagram = html.DocumentNode.SelectSingleNode("pre").InnerHtml;

            return anagram;
        }
    }
    
}
