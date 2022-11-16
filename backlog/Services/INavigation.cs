using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Services
{
    public interface INavigation
    {
        void NavigateTo<T>(object args = null);
        void GoBack<T>();
    }
}
