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
    public partial class NewsPage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            INode currentNode = Node.GetCurrent();
            var news = ModelFactory.CreateModel<Movies.Models.NewsPage>(currentNode);
            FormatDates(news.NewsListItems);
            pageContent.Text = MustacheHelper.RenderMustacheTemplate(this, "news", news);

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