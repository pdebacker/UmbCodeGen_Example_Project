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
    public partial class CinemasPage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            INode currentNode = Node.GetCurrent();
            var cinemas = ModelFactory.CreateModel<Cinemas>(currentNode);
            pageContent.Text = MustacheHelper.RenderMustacheTemplate(this, "cities", cinemas);

        }
    }
}