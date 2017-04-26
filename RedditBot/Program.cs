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
                try
                {
                    TokenBucket bucket = new TokenBucket(60, 60);                
                    RedditBot bot = new RedditBot(
                        ConfigurationManager.AppSettings["clientId"],
                        ConfigurationManager.AppSettings["clientSecret"], 
                        ConfigurationManager.AppSettings["RedditUsername"], 
                        ConfigurationManager.AppSettings["RedditPassword"], 
                        client,  
                        bucket, 
                        "0.01", 
                        "PrettyNiceBot", 
                        "sandboxtest/comments", 
                        "body" );
                    bot.StartBot();
                    Console.ReadKey();
                }
                catch (TooLowCapacityException exception)
                {
                    Console.WriteLine($"Error thrown '{exception.Message}' ");
                }
                catch (FaultyLoginException exception)
                {
                    Console.WriteLine($"Error thrown '{exception.Message}' ");
                }
            }
        }
    }
}