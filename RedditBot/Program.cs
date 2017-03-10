using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace RedditBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Authenticator auth = new Authenticator(ConfigurationManager.AppSettings["clientId"], ConfigurationManager.AppSettings["clientSecret"]);
            auth.DoAuthenticate(ConfigurationManager.AppSettings["RedditUsername"], ConfigurationManager.AppSettings["RedditPassword"], "0,01", "PrettyNiceBot");
            
            
            Console.ReadKey();
        }
    }
}
