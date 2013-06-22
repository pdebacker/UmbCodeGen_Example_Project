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
    public partial class HomePage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            INode currentNode = Node.GetCurrent();
            var homePage = ModelFactory.CreateModel<Movies.Models.HomePage>(currentNode);

            INode moviesNode = Node.GetNodeByXpath("//HomePage/Movies");
            foreach (INode movieNode in moviesNode.ChildrenAsList.Take(3))
            {
                homePage.TopMovies.Add(ModelFactory.CreateModel<MovieListItem>(movieNode));
            }

            INode newsNode = Node.GetNodeByXpath("//HomePage/NewsPage");
            foreach (INode newsItemNode in newsNode.ChildrenAsList.Take(3))
            {
                homePage.NewsListItems.Add(ModelFactory.CreateModel<NewsListItem>(newsItemNode));
            }
            FormatDates(homePage.NewsListItems);

            pageContent.Text = MustacheHelper.RenderMustacheTemplate(this, "home", homePage);
        }

        private void FormatDates(List<NewsListItem> items)
        {
            foreach (NewsListItem item in items)
            {
                item.DisplayDate = item.Date.ToString("dd MMMM yyyy");
            }
        }
    }
}