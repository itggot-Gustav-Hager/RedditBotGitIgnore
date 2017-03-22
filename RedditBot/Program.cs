﻿using System;
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
                RedditBot bot = new RedditBot(ConfigurationManager.AppSettings["clientId"], ConfigurationManager.AppSettings["clientSecret"]);
                bot.DoAuthenticate(client, ConfigurationManager.AppSettings["RedditUsername"], ConfigurationManager.AppSettings["RedditPassword"], "0.01", "PrettyNiceBot");
                var jObject = bot.ParseJsonGetListOfValues(bot.FetchJson(client, "sandboxtest"), "title");
                bot.postCommentIfContainsKeyword(client, "test", "That's pretty nice!", jObject);
                Console.ReadKey();
            }
        }
    }
}
