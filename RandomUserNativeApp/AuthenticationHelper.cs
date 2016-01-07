using System;
using System.Threading.Tasks;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace RandomUserNativeApp
{
    internal class AuthenticationHelper
    {
        public static string TokenForUser;

        /// <summary>
        /// Async task to acquire token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> AcquireTokenAsyncForUser()
        {
            return GetTokenForUser();
        }

        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static string GetTokenForUser()
        {
            if (TokenForUser == null)
            {
                AuthenticationContext authenticationContext = new AuthenticationContext(Constants.AuthString, false);
                AuthenticationResult userAuthnResult = authenticationContext.AcquireToken(
                    Constants.ResourceUrl,
                    Constants.ClientId, 
                    new Uri(Constants.ClientRedirectUri),
                    PromptBehavior.Auto);
                TokenForUser = userAuthnResult.AccessToken;
                Console.WriteLine("\nWelcome " + userAuthnResult.UserInfo.GivenName + " " +
                                  userAuthnResult.UserInfo.FamilyName);
            }
            return TokenForUser;
        }

        /// <summary>
        /// Get Active Directory Client for User.
        /// </summary>
        /// <returns>ActiveDirectoryClient for User.</returns>
        public static ActiveDirectoryClient GetActiveDirectoryClientAsUser()
        {
            Uri servicePointUri = new Uri(Constants.ResourceUrl);
            Uri serviceRoot = new Uri(servicePointUri, Constants.TenantId);
            ActiveDirectoryClient activeDirectoryClient = new ActiveDirectoryClient(serviceRoot,
                async () => await AcquireTokenAsyncForUser());
            return activeDirectoryClient;
        }
    }
}
