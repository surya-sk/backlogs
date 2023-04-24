using Backlogs.Models;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Backlogs.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LogsDialog : ContentDialog
    {
        List<Log> m_logs;
        public LogsDialog(List<Log> logs)
        {
            this.InitializeComponent();
            Debug.WriteLine(logs.Count);
            m_logs = logs;
        }
    }
}
