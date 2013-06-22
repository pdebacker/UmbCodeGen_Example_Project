using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Movies.Helpers;
using Movies.Models;
using umbraco.interfaces;
using umbraco.NodeFactory;

namespace Movies.masterpages
{
    public partial class NewsItemPage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            INode currentNode = Node.GetCurrent();
            var newsItem = ModelFactory.CreateModel<Movies.Models.NewsItem>(currentNode);
            newsItem.DisplayDate = newsItem.Date.ToString("dd MMMM yyyy");
            pageContent.Text = MustacheHelper.RenderMustacheTemplate(this, "newsItem", newsItem);
        }
    }
}