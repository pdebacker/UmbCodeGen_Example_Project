using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Movies.Models
{
    public class NavigationMenu
    {
        [Children("MenuItem")]
        public List<MenuItem> MenuItems { get; set; }
    }
}