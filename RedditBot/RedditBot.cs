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
        private string _username;
        private string _password;
        private TokenBucket _tokenBucket;
        private string _botVersion;
        private string _botName;
        private string _subreddit;
        private string _jsonKey;
        private HttpClient _client;

        /// <summary>
        /// A reddit bot
        /// </summary>
        /// <param name="clientId">the Id of the client</param>
        /// <param name="clientSecret">the Secret of the client</param>
        /// <param name="username">the reddit username</param>
        /// <param name="password">the reddit password</param>
        /// <param name="client">a HttpClient to be used</param>
        /// <param name="tokenBucket">a tokenBucket, from the class TokenBucket</param>
        /// <param name="botVersion">the version of the bot</param>
        /// <param name="botName">the name of the bot</param>
        /// <param name="subreddit">the subreddit that should be searcehd for information, example: "sandboxtest", different paths can be used, for example /new, /comments</param>
        /// <param name="jsonValue">the json key that is to be used</param>
        public RedditBot(string clientId, string clientSecret, string username, string password, HttpClient client, TokenBucket tokenBucket, string botVersion, string botName, string subreddit, string jsonValue)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _username = username;
            _password = password;
            _tokenBucket = tokenBucket;
            _botVersion = botVersion;
            _botName = botName;
            _subreddit = subreddit;
            _jsonKey = jsonValue;
            _client = client;
    }
        /// <summary>
        /// Starts the reddit bot
        /// </summary>
        public void StartBot()
        {
            Authenticate();
            var jObjects = ParseJsonGetListOfValues(FetchJson());
            Anagram anagramizer = new Anagram();
            int i = 0;
            while (i < 1)
            {
                foreach (var jObject in jObjects)
                {
                    if (_tokenBucket.RequestIsAllowed())
                    {
                        if (ContainsKeyword("!anagramize", jObject))
                        {
                            PostComment($"Anagram: \n{anagramizer.Anagramize(jObject.value.ToString())}", jObject);
                        }
                    }
                    else
                    {
                        int time = _tokenBucket.TimeUntilRefresh();
                        Console.WriteLine($"out of tokens, sleeping for {time} seconds");
                        System.Threading.Thread.Sleep(time*1000);
                    }
                }
            }

        }
        /// <summary>
        /// Authenticates reddit bot with clientId, clientSecret
        /// </summary
        public void Authenticate()
        {
            var authenticationArray = Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}");
            var encodedAuthenticationString = Convert.ToBase64String(authenticationArray);
            _client.DefaultRequestHeaders.Authorization = new
            System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedAuthenticationString);
            _client.DefaultRequestHeaders.Add("User-Agent", $"{_botName} /v{_botVersion} by {_username}");

            var formData = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", _username },
                { "password", _password }
            };
            var encodedFormData = new FormUrlEncodedContent(formData);
            var authUrl = "https://www.reddit.com/api/v1/access_token";
            var response = _client.PostAsync(authUrl, encodedFormData).GetAwaiter().GetResult();

            // Response Code
            Console.WriteLine(response.StatusCode);

            // Actual Token
            var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var accessToken = JObject.Parse(responseData).SelectToken("access_token").ToString();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
        }
        
        /// <summary>
        /// Takes HttpClient and a subredditurl, returns JObject
        /// </summary>
        /// <returns>a JObject with the Json</returns>
        public dynamic FetchJson()
        {
            var redditPageJsonResponse = _client.GetAsync(String.Format("https://oauth.reddit.com/r/{0}/",_subreddit)).GetAwaiter().GetResult();

            var redditPageJsonData = redditPageJsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            dynamic postData = JObject.Parse(redditPageJsonData);

            return postData;

        }

        /// <summary>
        /// Takes JObject and a value. Fetches the value from all children
        /// </summary>
        /// <param name="subredditJsonData">dynamic with Json from subreddit</param>
        /// <param name="value">Json Value</param>
        /// <returns>a list of a List of JValues</returns>
        public List<JObject> ParseJsonGetListOfValues(dynamic subredditJsonData)
        {

            List<JObject> redditObjectList = new List<JObject>();
            


            foreach (var post in subredditJsonData.data.children)
            {
                dynamic thingsToAddToThings = new JObject();
                thingsToAddToThings.value = post.data.SelectToken(_jsonKey);
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
        public void PostComment(string comment, JObject post)
        {
            var formData = new Dictionary<string, string>
                {
                    { "api_type", "json" },
                    { "text", comment },
                    { "thing_id", String.Format("{0}_{1}",post.SelectToken("kind"), post.SelectToken("id")) }
                };
            var encodedFormData = new FormUrlEncodedContent(formData);
            var authUrl = "https://oauth.reddit.com/api/comment";
            var response = _client.PostAsync(authUrl, encodedFormData).GetAwaiter().GetResult();
            Console.WriteLine(response.StatusCode);
            Console.WriteLine($"Your response to {post.SelectToken("id")} has been submitted!");
        }
        
    }
}
