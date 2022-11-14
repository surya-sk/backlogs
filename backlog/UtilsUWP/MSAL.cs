using Backlogs.Constants;
using Backlogs.Saving;
using Backlogs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Logger = Backlogs.Logging.Logger;

namespace Backlogs.Utils.UWP
{
    public class MSAL : IMsal
    {
        private const string CLIENT_ID = "c81b068d-ab10-4c00-a24d-08c3a1a6b7c6";
        private readonly string s_MSGraphURL = MSALConstants.MSGraphURL;
        private GraphServiceClient s_graphServiceClient = null;
        private IPublicClientApplication s_PublicClientApplication;

        private IUserSettings m_settings = App.Services.GetRequiredService<IUserSettings>();

        private string[] scopes = MSALConstants.Scopes;

        static StorageFolder s_cacheFolder = ApplicationData.Current.LocalCacheFolder;
        static string m_accountPicFile = "profile.png";
        
        public async Task<GraphServiceClient> GetGraphServiceClient()
        {
            if (s_graphServiceClient == null)
            {
                s_graphServiceClient = await SignInAndInitializeGraphServiceClient().ConfigureAwait(false);
                try
                {
                    await Logger.Info("Fetching graph service client.....");
                    var user = await s_graphServiceClient.Me.Request().GetAsync();
                    m_settings.Set(SettingsConstants.UserName, user.GivenName);
                    try
                    {
                        Stream photoresponse = await s_graphServiceClient.Me.Photo.Content.Request().GetAsync();
                        if (photoresponse != null)
                        {
                            using (var randomAccessStream = photoresponse.AsRandomAccessStream())
                            {
                                BitmapImage image = new BitmapImage();
                                randomAccessStream.Seek(0);
                                await image.SetSourceAsync(randomAccessStream);

                                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
                                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                                var storageFile = await s_cacheFolder.CreateFileAsync(m_accountPicFile, CreationCollisionOption.ReplaceExisting);
                                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, await storageFile.OpenAsync(FileAccessMode.ReadWrite));
                                encoder.SetSoftwareBitmap(softwareBitmap);
                                await encoder.FlushAsync();
                            }
                        }
                    }
                    catch (ServiceException ex)
                    {
                        await Logger.Error("Failed to fetch user photo", ex);
                    }
                }
                catch (Exception ex)
                {
                    await Logger.Error("Failed to sign-in user or get user photo and name", ex);
                }
            }
            return s_graphServiceClient;
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
                m_settings.Set(SettingsConstants.IsSignedIn, true);   
            }
            return authResult.AccessToken;
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
            var accounts = await s_PublicClientApplication.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();
            try
            {
                await Logger.Info("Signing out user...");
                await s_PublicClientApplication.RemoveAsync(firstAccount).ConfigureAwait(false);
                m_settings.Set(SettingsConstants.IsSignedIn, false);
                try
                {
                    await SaveData.GetInstance().DeleteLocalFileAsync();
                }
                catch
                {
                    // : )
                }
            }
            catch (Exception ex)
            {
                await Logger.Error("Failed to sign out user.", ex);
            }
        }
    }
}
