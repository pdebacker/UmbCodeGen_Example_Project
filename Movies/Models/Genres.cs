﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Movies.Models
{
    public partial class Genres
    {
        [Children("Genre")]
        public List<GenreFilterItem> GenreFilterItems { get; set; }
    }
}