using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Movies.Models
{
    public partial class NewsPage
    {
        [Children("NewsItem")]
        public List<NewsListItem> NewsListItems { get; set; }
    }
}