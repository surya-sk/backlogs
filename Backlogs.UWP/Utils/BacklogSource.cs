using Backlogs.Models;
using Microsoft.Toolkit.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backlogs.Utils.UWP
{
    public class BacklogSource : IIncrementalSource<Backlog>
    {
        private ObservableCollection<Backlog> m_backlogs;
        public BacklogSource(ObservableCollection<Backlog> backlogs)
        {
            m_backlogs = backlogs;
        }

        public async Task<IEnumerable<Backlog>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            var result = (from b in m_backlogs select b).Skip(pageIndex * pageSize).Take(pageSize);
            await Task.Delay(100);
            return result;
        }
    }
}
