﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Movies.Models
{
    public partial class Movie 
    {
        [Property("movieGenre")]
        public Genre GenreInfo { get; set; }

        [Children("Paragraph", true)]
        public List<Paragraph> Paragraphs { get; set; }

        [Property("movieRelatedMovies")]
        public List<MovieListItem> RelatedMovieItems { get; set; }

        public bool HasRelatedMovieItems { get; set; }

        public List<Cinema> Cinemas { get; set; }
    }
}