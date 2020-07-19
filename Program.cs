using System;
using System.Threading.Tasks;

namespace BetterTwitter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string consumerKey = "";
            string consumerSecret = "";
            string accessToken = "";
            string accessTokenSecret = "";

            TwitterClient twitterClient = new TwitterClient(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            await twitterClient.SetBearerToken();

            await twitterClient.SetInactiveFriends("sercandumansiz", 200, true, "");

            await twitterClient.DestroyFriendships();
        }
    }
}
