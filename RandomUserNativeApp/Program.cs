#region

using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Microsoft.Azure.ActiveDirectory.GraphClient;

#endregion

namespace RandomUserNativeApp
{
    public class Program
    {
        // Single-Threaded Apartment required for OAuth2 Authz Code flow (User Authn) to execute for this demo app
        [STAThread]
        private static void Main()
        {
            // record start DateTime of execution
            string currentDateTime = DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture);
            
            var activeDirectoryClient = AuthenticationHelper.GetActiveDirectoryClientAsUser();

            #region TenantDetails
            //*********************************************************************
            // Get Tenant Details
            // Note: update the string TenantId with your TenantId.
            // This can be retrieved from the login Federation Metadata end point:             
            // https://login.windows.net/GraphDir1.onmicrosoft.com/FederationMetadata/2007-06/FederationMetadata.xml
            //  Replace "GraphDir1.onMicrosoft.com" with any domain owned by your organization
            // The returned value from the first xml node "EntityDescriptor", will have a STS URL
            // containing your TenantId e.g. "https://sts.windows.net/4fd2b2f2-ea27-4fe5-a8f3-7b1a7c975f34/" is returned for GraphDir1.onMicrosoft.com
            //*********************************************************************
            VerifiedDomain defaultDomain = new VerifiedDomain();
            ITenantDetail tenant = null;
            Console.WriteLine("\n Retrieving Tenant Details");
            try
            {
                List<ITenantDetail> tenantsList = activeDirectoryClient.TenantDetails
                    .Where(tenantDetail => tenantDetail.ObjectId.Equals(Constants.TenantId))
                    .ExecuteAsync().Result.CurrentPage.ToList();
                if (tenantsList.Count > 0)
                {
                    tenant = tenantsList.First();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError getting TenantDetails {0} {1}", e.Message,
                    e.InnerException != null ? e.InnerException.Message : "");
            }

            if (tenant == null)
            {
                Console.WriteLine("Tenant not found");
            }
            else
            {
                TenantDetail tenantDetail = (TenantDetail)tenant;
                Console.WriteLine("Tenant Display Name: " + tenantDetail.DisplayName);

                // Get the Tenant's Verified Domains 
                var initialDomain = tenantDetail.VerifiedDomains.First(x => x.Initial.HasValue && x.Initial.Value);
                Console.WriteLine("Initial Domain Name: " + initialDomain.Name);
                defaultDomain = tenantDetail.VerifiedDomains.First(x => x.@default.HasValue && x.@default.Value);
                Console.WriteLine("Default Domain Name: " + defaultDomain.Name);

                // Get Tenant's Tech Contacts
                foreach (string techContact in tenantDetail.TechnicalNotificationMails)
                {
                    Console.WriteLine("Tenant Tech Contact: " + techContact);
                }
            }
            #endregion



            #region Create a random users
            try
            {
                var batchSize = 0;
                foreach (var randomUser in RandomUserGeneratorClient.GenerateRandomUsers(defaultDomain.Name, 100))
                {
                    Console.WriteLine("\nCreating creating new user: " + randomUser.User.DisplayName);
                    activeDirectoryClient.Users.AddUserAsync(randomUser.User, true).Wait();
                    batchSize ++;
                    if (batchSize % 5 == 0)
                    {
                        activeDirectoryClient.Users.ExecuteAsync().Wait();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError creating new user {0} {1}", e.Message,
                    e.InnerException != null ? e.InnerException.Message : "");
            }
            #endregion

            //*********************************************************************************************
            // End of Demo Console App
            //*********************************************************************************************
            Console.WriteLine("\nCompleted at {0} \n Press Any Key to Exit.", currentDateTime);
            Console.ReadKey();
        }
    }
}