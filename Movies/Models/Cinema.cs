using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Movies.Models
{
    public partial class Cinema
    {
        [Children("MovieProgram")]
        public List<MovieProgram> MoviePrograms { get; set; }
    }
}