using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backlog.Utils
{
    public class FilmResult
    {
        public string searchType { get; set; }
        public string expression { get; set; }
        public Film[] results { get; set; }
    }

    public class Film
    {
        public string id { get; set; }
        public string resultType { get; set; }
        public string image { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }
}
