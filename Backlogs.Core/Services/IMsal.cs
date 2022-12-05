using Microsoft.Graph;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface IMsal
    {
        Task<string> SignInAndGetAuthResult();
        Task<GraphServiceClient> SignInAndInitializeGraphServiceClient();
        Task<GraphServiceClient> GetGraphServiceClient();
        Task SignOut();
    }
}
