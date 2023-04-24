using System;
using System.Collections.Generic;
using System.Text;

namespace Backlogs.Models
{
    public class Log
    {
        public string Date { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Date}---{Message}\n";
        }
    }
}
