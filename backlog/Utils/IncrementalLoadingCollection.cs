using backlog.Models;
using backlog.Saving;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Toolkit.Uwp;
using Windows.UI.Xaml.Data;
using Microsoft.Toolkit.Collections;
using System.Threading;

namespace backlog.Utils
{
    public class BacklogSource : IIncrementalSource<Backlog>
    {
        private readonly List<Backlog> m_backlogs;

        public BacklogSource(ObservableCollection<Backlog> backlogs)
        {
            m_backlogs = SaveData.GetInstance().GetIncompleteBacklogs().ToList();
        }

        public async Task<IEnumerable<Backlog>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var result = (from b in m_backlogs select b).Skip(pageIndex * pageSize).Take(pageSize);
            await Task.Delay(100);
            return result;
        }
    }
}
