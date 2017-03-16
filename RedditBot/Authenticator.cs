using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditBot
{
    class Authenticator
    {
        private string _clientId;
        private string _clientSecret;
        /// <summary>
        /// An authenticator for reddit
        /// </summary>
        /// <param name="clientId">Reddit clientID</param>
        /// <param name="clientSecret">Reddit clientSecret</param>
        public Authenticator(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }
        /// <summary>
        /// Authenticates reddit bot with clientId, clientSecret
        ///
        ///
        /// </summary>
        /// <param name="redditUsername">Reddit username to the account of the bot</param>
        /// <param name="redditPassword">Reddit password to the account of the bot</param>
        /// <param name="clientVersion">Version of the client</param>
        /// <param name="botName">the name of the bot, to be used in description</param>
        public void DoAuthenticate(string redditUsername, string redditPassword, string clientVersion, string botName) {
            using (var client = new HttpClient())
            {
                var authenticationArray = Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}");
                var encodedAuthenticationString = Convert.ToBase64String(authenticationArray);
                client.DefaultRequestHeaders.Authorization = new
                System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedAuthenticationString);
                client.DefaultRequestHeaders.Add("User-Agent", $"{botName} /v{clientVersion} by {redditUsername}");

                var formData = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", redditUsername },
                    { "password", redditPassword }
                };
                var encodedFormData = new FormUrlEncodedContent(formData);
                var authUrl = "https://www.reddit.com/api/v1/access_token";
                var response = client.PostAsync(authUrl, encodedFormData).GetAwaiter().GetResult();

                // Response Code
                Console.WriteLine(response.StatusCode);

                // Actual Token
                var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var accessToken = JObject.Parse(responseData).SelectToken("access_token").ToString();

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
                ParseJsonGetListOfValues(FetchJson(client, "https://reddit.com/r/sweden/new.json"), "title");
            }
        }
        /// <summary>
        /// Takes HttpClient and a subredditurl, returns JObject
        /// </summary>
        /// <param name="client">HttpClient</param>
        /// <param name="url">The url to the subreddit</param>
        /// <returns>a JObject with the Json</returns>
        private dynamic FetchJson(HttpClient client, string url)
        {
            var redditPageJsonResponse = client.GetAsync(url).GetAwaiter().GetResult();

            var redditPageJsonData = redditPageJsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            dynamic postData = JObject.Parse(redditPageJsonData);

            return postData;

        }

        /// <summary>
        /// Takes JObject and a value. Fetches the value from all children
        /// </summary>
        /// <param name="subredditJsonData">JObject with Json from subreddit</param>
        /// <param name="value">Json Value</param>
        /// <returns>a list of JValues</returns>
        private List<JValue> ParseJsonGetListOfValues(dynamic subredditJsonData, string value)
        {

            List<JValue> things = new List<JValue>();

            foreach (var post in subredditJsonData.data.children)
            {
                things.Add(post.data.SelectToken(value));
            }

            //noob test pls dont judge

            foreach (var node in things)
            {
                Console.WriteLine(node);
            }
            return things;
        }
    }
}
