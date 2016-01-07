using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Newtonsoft.Json.Linq;

namespace RandomUserNativeApp
{
    public class RandomUser
    {
        public User User { get; set; }
        public Uri ThumbnailPhotoUri { get; set; }
    }

    internal class RandomUserGeneratorClient
    {
        private static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static IEnumerable<RandomUser> GenerateRandomUsers(string domain, int count)
        {
            var httpClient = new HttpClient();
            var response = httpClient.GetAsync("http://api.randomuser.me/?results="+count).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Failed to communicate with RandomUser service");
            }
            
            var results = JObject.Parse(response.Content.ReadAsStringAsync().Result)["results"];
            foreach (var result in results)
            {
                var user = result["user"];
                var firstName = UppercaseFirst((string) user["name"]["first"]);
                var lastName = UppercaseFirst((string) user["name"]["last"]);
                yield return new RandomUser()
                {
                    User = new User()
                    {
                        MailNickname = (string) user["username"],
                        UserPrincipalName = user["username"] + "@" + domain,
                        GivenName = firstName,
                        Surname = lastName,
                        DisplayName = firstName + " " + lastName,
                        StreetAddress = (string) user["location"]["street"],
                        City = (string) user["location"]["city"],
                        State = (string) user["location"]["state"],
                        PostalCode = (string) user["location"]["zip"],
                        Country = (string) user["nationality"],
                        Mobile = (string) user["cell"],
                        PasswordProfile = new PasswordProfile
                        {
                            Password = "Test1234",
                            ForceChangePasswordNextLogin = true
                        },
                        AccountEnabled = true
                    },
                    ThumbnailPhotoUri = new Uri((string) user["picture"]["thumbnail"])
                };
            }
        }
    }
}
