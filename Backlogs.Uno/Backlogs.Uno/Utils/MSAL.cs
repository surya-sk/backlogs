using Backlogs.Constants;
using Backlogs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Backlogs.Uno;
using System.Diagnostics;
using Uno.UI.MSAL;

namespace Backlogs.Utils.Uno
{
    public class MSAL : IMsal
    {
        private const string CLIENT_ID = "c81b068d-ab10-4c00-a24d-08c3a1a6b7c6";
        private readonly string s_MSGraphURL = MSALConstants.MSGraphURL;
        private GraphServiceClient? s_graphServiceClient = null;
        private IPublicClientApplication? s_PublicClientApplication;

        private IUserSettings m_settings = App.Services.GetRequiredService<IUserSettings>();
        private IFileHandler m_fileHandler = App.Services.GetRequiredService<IFileHandler>();

        private string[] scopes = MSALConstants.Scopes;
        
        public async Task<GraphServiceClient> GetGraphServiceClient()
        {
            if (s_graphServiceClient == null)
            {
                s_graphServiceClient = await SignInAndInitializeGraphServiceClient().ConfigureAwait(false);
                try
                {
                    await m_fileHandler.WriteLogsAsync("Fetching graph service client.....");
                    var user = await s_graphServiceClient.Me.Request().GetAsync();
                    m_settings.Set(SettingsConstants.UserName, user.GivenName);
                    try
                    {
                        Stream photoresponse = await s_graphServiceClient.Me.Photo.Content.Request().GetAsync();
                        if (photoresponse != null)
                        {
                            await m_fileHandler.WriteBitmapAsync(photoresponse, "profile.png");
                        }
                    }
                    catch (ServiceException ex)
                    {
                        await m_fileHandler.WriteLogsAsync("Failed to fetch user photo", ex);
                    }
                }
                catch (Exception ex)
                {
                    await m_fileHandler.WriteLogsAsync("Failed to sign-in user or get user photo and name", ex);
                }
            }
            return s_graphServiceClient;
        }

        public async Task<string?> SignInAndGetAuthResult()
        {
            string sid = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper();

            // the redirect uri you need to register
            string redirectUri = $"ms-appx-web://microsoft.aad.brokerplugin/S-1-15-2-2566872105-1906516075-403359635-2971900813-1913047554-2806970718-2761120688";

            AuthenticationResult? authResult;

            s_PublicClientApplication = PublicClientApplicationBuilder.Create(CLIENT_ID)
                            .WithBroker(true)
                            .WithRedirectUri(redirectUri)
                            .WithUnoHelpers()
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
                 .WithUseEmbeddedWebView(true)
                 .WithUnoHelpers()
                 .ExecuteAsync();
            }
            //if (authResult != null)
            //{
            //    m_settings.Set(SettingsConstants.IsSignedIn, true);   
            //}
            return authResult?.AccessToken;
        }

        public async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient()
        {
            GraphServiceClient graphClient = new GraphServiceClient(s_MSGraphURL,
            new DelegateAuthenticationProvider(async (requestMessage) => {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", await SignInAndGetAuthResult());
            }));

            return await Task.FromResult(graphClient);
        }

        public async Task SignOut()
        {
            //var accounts = await s_PublicClientApplication.GetAccountsAsync();
            //IAccount firstAccount = accounts.FirstOrDefault();
            //try
            //{
            //    await m_fileHandler.WriteLogsAsync("Signing out user...");
            //    await s_PublicClientApplication.RemoveAsync(firstAccount).ConfigureAwait(false);
            //    m_settings.Set(SettingsConstants.IsSignedIn, false);
            //    try
            //    {
            //        //await SaveData.GetInstance().DeleteLocalFileAsync();
            //    }
            //    catch
            //    {
            //        // : )
            //    }
            //}
            //catch (Exception ex)
            //{
            //    await m_fileHandler.WriteLogsAsync("Failed to sign out user.", ex);
            //}
            throw new NotImplementedException();
        }
    }
}
