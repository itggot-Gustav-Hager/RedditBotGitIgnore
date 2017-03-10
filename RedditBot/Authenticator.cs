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

                client.GetAsync("https://oauth.reddit.com/api/v1/me").GetAwaiter().GetResult();
                responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.WriteLine(responseData);
                Console.ReadKey();
            }
        }
    }
}
