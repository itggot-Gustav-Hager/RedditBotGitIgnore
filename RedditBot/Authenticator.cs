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
    class RedditBot
    {
        private string _clientId;
        private string _clientSecret;
        /// <summary>
        /// An authenticator for reddit
        /// </summary>
        /// <param name="clientId">Reddit clientID</param>
        /// <param name="clientSecret">Reddit clientSecret</param>
        public RedditBot(string clientId, string clientSecret)
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
        public void DoAuthenticate(HttpClient client, string redditUsername, string redditPassword, string clientVersion, string botName)
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
        }
        
        /// <summary>
        /// Takes HttpClient and a subredditurl, returns JObject
        /// </summary>
        /// <param name="client">HttpClient</param>
        /// <param name="url">The url to the subreddit</param>
        /// <returns>a JObject with the Json</returns>
        public dynamic FetchJson(HttpClient client, string subreddit)
        {
            var redditPageJsonResponse = client.GetAsync(String.Format("https://oauth.reddit.com/r/{0}/",subreddit)).GetAwaiter().GetResult();

            var redditPageJsonData = redditPageJsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            dynamic postData = JObject.Parse(redditPageJsonData);

            return postData;

        }

        /// <summary>
        /// Takes JObject and a value. Fetches the value from all children
        /// </summary>
        /// <param name="subredditJsonData">JObject with Json from subreddit</param>
        /// <param name="value">Json Value</param>
        /// <returns>a list of a List of JValues</returns>
        public List<JObject> ParseJsonGetListOfValues(dynamic subredditJsonData, string value)
        {

            List<JObject> redditObjectList = new List<JObject>();
            


            foreach (var post in subredditJsonData.data.children)
            {
                dynamic thingsToAddToThings = new JObject();
                thingsToAddToThings.value = post.data.SelectToken(value);
                thingsToAddToThings.kind = post.kind;
                thingsToAddToThings.id = post.data.id;
                redditObjectList.Add(thingsToAddToThings);
            }

            return redditObjectList;
        }

        public void postCommentIfContainsKeyword(HttpClient client, string keyword, string comment, List<JObject> posts)
        {
            foreach(var post in posts)
            {
                if (post.SelectToken("value").ToString().Contains(keyword))
                {
                    var formData = new Dictionary<string, string>
                    {
                        { "api_type", "json" },
                        { "text", comment },
                        { "thing_id", String.Format("{0}_{1}",post.SelectToken("kind"), post.SelectToken("id")) }
                    };
                    var encodedFormData = new FormUrlEncodedContent(formData);
                    var authUrl = "https://oauth.reddit.com/api/comment";
                    var response = client.PostAsync(authUrl, encodedFormData).GetAwaiter().GetResult();
                    Console.WriteLine(response.StatusCode);
                }
                else
                {
                    Console.WriteLine("your keyword wasn't found");
                }
            }
            
            
        }
    }
}
