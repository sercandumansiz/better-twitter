using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Twitter.OAuth;

namespace BetterTwitter
{
    public class TwitterClient
    {
        public const string DATE_FORMAT = "ddd MMM dd HH:mm:ss +ffff yyyy";
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;
        private string bearerToken;
        private readonly HttpClient _httpClient;
        private List<User> inactiveUsers = new List<User>();

        public TwitterClient(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
            _accessTokenSecret = accessTokenSecret;

            _httpClient = new HttpClient();
        }

        public async Task SetBearerToken()
        {
            _httpClient.DefaultRequestHeaders.Clear();

            string authorizationHeaderValue = $"{HttpUtility.UrlEncode(_consumerKey)}:{HttpUtility.UrlEncode(_consumerSecret)}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                                                                  AuthenticationSchemes.Basic.ToString(),
                                                                  $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationHeaderValue))}");

            HttpContent request = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpResponseMessage response = await _httpClient.PostAsync("https://api.twitter.com/oauth2/token", request);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                bearerToken = JsonSerializer.Deserialize<Token>(content).Value;

                Console.WriteLine(content);
            }
        }

        public async Task SetInactiveFriends(string screenName, int count = 200, bool includeEntities = false, string cursor = "")
        {
            string url = $"https://api.twitter.com/1.1/friends/list.json?screen_name={screenName}&count={count}&include_entities={includeEntities}";

            if (!string.IsNullOrEmpty(cursor))
            {
                url += $"&cursor={cursor}";
            }

            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);


            HttpResponseMessage response = await _httpClient.GetAsync(url);

            string content = await response.Content.ReadAsStringAsync();

            PaginatedUser paginatedUser = JsonSerializer.Deserialize<PaginatedUser>(content);

            if (paginatedUser.Users != null)
            {
                var users = paginatedUser.Users.Where(u => u.Status != null).ToList();

                if (users.Count > 0)
                {
                    foreach (var user in users)
                    {
                        DateTime lastActiveDate = DateTime.ParseExact(user.Status.CreatedAt, DATE_FORMAT, new System.Globalization.CultureInfo("en-US"));

                        if (lastActiveDate < DateTime.Now.AddMonths(-6))
                        {
                            inactiveUsers.Add(user);
                        }
                    }

                    if (!string.IsNullOrEmpty(paginatedUser.NextCursor))
                    {
                        await SetInactiveFriends(screenName, count, includeEntities, paginatedUser.NextCursor);
                    }
                }
            }
        }

        public async Task DestroyFriendships()
        {
            List<string> unfollowedUsers = new List<string>();

            foreach (var inactiveUser in inactiveUsers)
            {
                _httpClient.DefaultRequestHeaders.Clear();

                OAuthHeaderGenerator oAuthHeaderGenerator = new OAuthHeaderGenerator(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret);

                string url = $"https://api.twitter.com/1.1/friendships/destroy.json?screen_name={inactiveUser.Name}";

                string oAuth = oAuthHeaderGenerator.GenerateOAuthHeader("POST", url);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", oAuth);

                try
                {
                    HttpResponseMessage response = await _httpClient.PostAsync(url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        unfollowedUsers.Add(inactiveUser.Name);
                    }
                    else
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(content);
                    }
                }
                catch (Exception ex)
                {
                    File.WriteAllLines("unfollowed-users.txt", unfollowedUsers);
                }
            }

            File.WriteAllLines("unfollowed-users.txt", unfollowedUsers);
        }
    }
}