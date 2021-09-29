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

    public class SeriesResult
    {
        public string searchType { get; set; }
        public string expression { get; set; }
        public SeriesResponse[] results { get; set; }
    }

    public class SeriesResponse
    {
        public string id { get; set; }
        public string resultType { get; set; }
        public string image { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }

    public class Series
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


    public class GameResponse
    {
        public int id { get; set; }
    }

    public class GameResult
    {
        public int id { get; set; }
        public int cover { get; set; }
        public List<int> involved_companies { get; set; }
        public string name { get; set; }
        public List<int> release_dates { get; set; }
        public string storyline { get; set; }
    }

    public class InvovledGameCompanies
    {
        public int company { get; set; }
    }

    public class GameCompany
    {
        public string name { get; set; }
    }

    public class GameCover
    {
        public string url { get; set; }
    }

    public class GameReleaseDate
    {
        public long date { get; set; }
    }

    public class Game
    {
        public string name { get; set; }
        public string company { get; set; }
        public string storyline { get; set; }
        public string releaseDate { get; set; }
        public string image { get; set; }
    }

}
