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
        /// <param name="url">The subreddit and eventual /parameters</param>
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
        /// <summary>
        /// checks if the selected value in a post contains a keyword
        /// </summary>
        /// <param name="keyword">the keyword that the JValue value should be searched for</param>
        /// <param name="post">The JObject whoms JValue is to be searched</param>
        /// <returns></returns>
        public bool ContainsKeyword(string keyword, JObject post)
        {
            if (post.SelectToken("value").ToString().ToLower().Contains(keyword))
            {
                return true;
            }
            else
            {
                Console.WriteLine("Your keyword wasn't found");
                return false;
            }            
        }
        /// <summary>
        /// Posts a comment to reddit
        /// </summary>
        /// <param name="client">the HttpClient that is to be used</param>
        /// <param name="comment">The comment which is to be commented</param>
        /// <param name="post">The post that is answered to</param>
        public void PostComment(HttpClient client, string comment, JObject post)
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
            Console.WriteLine("Your post has been submitted!");
        }
        
    }
}
