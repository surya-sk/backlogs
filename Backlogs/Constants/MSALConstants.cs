using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backlogs.Constants
{
    public static class MSALConstants
    {
        public static readonly string[] Scopes = new string[]
        {
             "user.read",
             "Files.Read",
             "Files.Read.All",
             "Files.ReadWrite",
             "Files.ReadWrite.All"
        };
        public static readonly string MSGraphURL = "https://graph.microsoft.com/v1.0/";
    }
}
