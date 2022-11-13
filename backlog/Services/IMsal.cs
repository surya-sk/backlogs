using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
