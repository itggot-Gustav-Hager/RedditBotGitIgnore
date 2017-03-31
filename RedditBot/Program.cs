using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http;

namespace RedditBot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (HttpClient client = new HttpClient())
            {
                TokenBucket bucket = new TokenBucket(60, 60);
                RedditBot bot = new RedditBot(ConfigurationManager.AppSettings["clientId"], ConfigurationManager.AppSettings["clientSecret"]);
                bot.DoAuthenticate(client, ConfigurationManager.AppSettings["RedditUsername"], ConfigurationManager.AppSettings["RedditPassword"], "0.01", "PrettyNiceBot");
                var jObjects = bot.ParseJsonGetListOfValues(bot.FetchJson(client, "all/comments"), "body");
                //foreach(var jObject in jObjects)
                //{
                //    if (bucket.RequestIsAllowed())
                //    {
                //        bot.postCommentIfContainsKeyword(client, "what", "That's pretty nice!", jObject);
                //    }
                //}
                Anagram anagramination = new Anagram();
                anagramination.anagramize("gustav ebola gustav ebola abba ebollalalala");
                
                Console.ReadKey();
            }
        }
    }
}