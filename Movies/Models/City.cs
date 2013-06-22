using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Movies.Models
{
    public partial class City
    {
        [Children("Cinema", false)]
        public List<Cinema> Cinemas { get; set; }
    }
}