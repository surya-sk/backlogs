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
        public FilmResponse[] results { get; set; }
    }

    public class FilmResponse
    {
        public string id { get; set; }
        public string resultType { get; set; }
        public string image { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }

    public class Film
    {
        public string id { get; set; }
        public string originalTitle { get; set; }
        public string image { get; set; }
        public string fullTitle { get; set; }
        public string plot { get; set; }
        public string releaseDate { get; set; }
        public string runtimeStr { get; set; }
        public string directors { get; set; }
    }

    public class TVResult
    {
        public string searchType { get; set; }
        public string expression { get; set; }
        public Series[] results { get; set; }
    }

    public class Series
    {
        public string id { get; set; }
        public string resultType { get; set; }
        public string image { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }
}
