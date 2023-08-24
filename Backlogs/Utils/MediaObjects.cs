﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Backlogs.Utils
{
    public class FilmResult
    {
        public List<Film> results { get; set; }
        public int total_results { get; set; }
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
        public bool adult { get; set; }
        public string backdrop_path { get; set; }
        public List<int> genre_ids { get; set; }
        public int id { get; set; }
        public string original_language { get; set; }
        public string original_title { get; set; }
        public string overview { get; set; }
        public double popularity { get; set; }
        public string poster_path { get; set; }
        public string release_date { get; set; }
        public string title { get; set; }
        public bool video { get; set; }
        public double vote_average { get; set; }
        public int vote_count { get; set; }
    }

    public class SeriesResult
    {
        public List<Series> results { get; set; }
        public int total_results { get; set; }
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
        public bool adult { get; set; }
        public string backdrop_path { get; set; }
        public List<int> genre_ids { get; set; }
        public int id { get; set; }
        public List<string> origin_country { get; set; }
        public string original_language { get; set; }
        public string original_name { get; set; }
        public string overview { get; set; }
        public double popularity { get; set; }
        public string poster_path { get; set; }
        public string first_air_date { get; set; }
        public string name { get; set; }
        public double vote_average { get; set; }
        public int vote_count { get; set; }
    }

    public class TvSeriesInfo
    {
        public string Creators { set; get; }
        public List<string> Seasons { get; set; }
    }

    public class GameResponse
    {
        public int id { get; set; }
        public string name { get; set; }
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



    public class ImageLinks
    {
        public string smallThumbnail { get; set; }
        public string thumbnail { get; set; }
    }

    public class VolumeInfo
    {
        public string title { get; set; }
        public List<string> authors { get; set; }
        public string publisher { get; set; }
        public string publishedDate { get; set; }
        public string description { get; set; }
        public int pageCount { get; set; }
        public string printType { get; set; }
        public List<string> categories { get; set; }
        public double averageRating { get; set; }
        public int ratingsCount { get; set; }
        public string maturityRating { get; set; }
        public bool allowAnonLogging { get; set; }
        public string contentVersion { get; set; }
        public ImageLinks imageLinks { get; set; }
        public string language { get; set; }
        public string previewLink { get; set; }
        public string infoLink { get; set; }
        public string canonicalVolumeLink { get; set; }
    }



    public class Item
    {
        public string kind { get; set; }
        public string id { get; set; }
        public string etag { get; set; }
        public string selfLink { get; set; }
        public VolumeInfo volumeInfo { get; set; }
    }

    public class BookInfo
    {
        public string kind { get; set; }
        public int totalItems { get; set; }
        public List<Item> items { get; set; }
    }

    public class Book
    {
        public string name { get; set; }
        public string author { get; set; }
        public string releaseDate { get; set; }
        public string desciption { get; set; }
        public string image { get; set; }
        public int length { get; set; }
    }


    public class Music
    {
        public string name { get; set; }
        public string artist { get; set; }
        public string releaseDate { get; set; }
        public string description { get; set; }
        public string image { get; set; }
    }

    public class Wiki
    {
        public string published { get; set; }
        public string content { get; set; }
        public string summary { get; set; }
    }

    public class Artist
    {
        public string name { get; set; }
    }

    public class Image
    {
        public string size { get; set; }

        [JsonProperty("#text")]
        public string Text { get; set; }
    }

    public class Tag
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Album
    {
        public Wiki wiki { get; set; }
        public List<Image> image { get; set; }
        public string url { get; set; }
        public string artist { get; set; }
        public string name { get; set; }
        public string mbid { get; set; }
    }

    public class MusicData
    {
        public Album album { get; set; }
    }



}
