using Backlogs.Constants;
using Backlogs.Services;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Storage;

namespace Backlogs.Utils.UWP
{
    public class MSAL : IMsal
    {
        private const string CLIENT_ID = "c81b068d-ab10-4c00-a24d-08c3a1a6b7c6";
        private readonly string s_MSGraphURL = MSALConstants.MSGraphURL;
        private GraphServiceClient s_graphServiceClient = null;
        private IPublicClientApplication s_PublicClientApplication;

        private string[] scopes = MSALConstants.Scopes;

        static StorageFolder s_cacheFolder = ApplicationData.Current.LocalCacheFolder;
        static string m_accountPicFile = "profile.png";

        public Task<GraphServiceClient> GetGraphServiceClient()
        {
            throw new NotImplementedException();
        }

        public async Task<string> SignInAndGetAuthResult()
        {
            string sid = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper();

            // the redirect uri you need to register
            string redirectUri = $"ms-appx-web://microsoft.aad.brokerplugin/S-1-15-2-2566872105-1906516075-403359635-2971900813-1913047554-2806970718-2761120688";

            AuthenticationResult authResult;

            s_PublicClientApplication = PublicClientApplicationBuilder.Create(CLIENT_ID)
                            .WithBroker(true)
                            .WithRedirectUri(redirectUri)
                            .Build();
            var accounts = await s_PublicClientApplication.GetAccountsAsync();
            var accountToLogin = accounts.FirstOrDefault();
            try
            {
                // 4. AcquireTokenSilent 
                authResult = await s_PublicClientApplication.AcquireTokenSilent(scopes, accountToLogin)
                                          .ExecuteAsync();
            }
            catch (MsalUiRequiredException) // no change in the pattern
            {
                authResult = await s_PublicClientApplication.AcquireTokenInteractive(scopes)
                 .WithAccount(accountToLogin)  // this already exists in MSAL, but it is more important for WAM
                 .ExecuteAsync();
            }
            if (authResult != null)
            {
                
            }
            return authResult.AccessToken;
        }

        public Task<GraphServiceClient> SignInAndInitializeGraphServiceClient()
        {
            throw new NotImplementedException();
        }

        public Task SignOut()
        {
            throw new NotImplementedException();
        }
    }
}
