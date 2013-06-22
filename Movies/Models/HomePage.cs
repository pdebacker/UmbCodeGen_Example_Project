using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Movies.Models
{
    public partial class HomePage
    {
        public HomePage()
        {
            TopMovies = new List<MovieListItem>();
            NewsListItems = new List<NewsListItem>();
        }
        public List<MovieListItem> TopMovies { get; set; }
        public List<NewsListItem> NewsListItems { get; set; }
    }
}