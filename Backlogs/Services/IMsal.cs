using Microsoft.Graph;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    /// <summary>
    /// Handles signing the user in and out of their Microsoft account
    /// </summary>
    public interface IMsal
    {
        /// <summary>
        /// Signs the user into their MS account and gets the auth token
        /// </summary>
        /// <returns>the MSAL token</returns>
        Task<string> SignInAndGetAuthResult();

        /// <summary>
        /// Sign into MS Graph and obtains the GraphClient
        /// </summary>
        /// <returns>the graph service client</returns>
        Task<GraphServiceClient> SignInAndInitializeGraphServiceClient();

        /// <summary>
        /// Gets the obtained Graph Service Client
        /// </summary>
        /// <returns>the Graph Service Client</returns>
        Task<GraphServiceClient> GetGraphServiceClient();

        /// <summary>
        /// Signs the user out of their MS account
        /// </summary>
        /// <returns></returns>
        Task SignOut();
    }
}
