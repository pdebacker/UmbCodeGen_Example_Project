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

namespace Movies.usercontrols.Navigation
{
    public partial class MainNavigation : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Node node = new Node(1080); 
            var mainMenu = ModelFactory.CreateModel<NavigationMenu>(node);
            DetermineCurrentItem(mainMenu);
            topNavigationContent.Text = MustacheHelper.RenderMustacheTemplate(this, "topNavigation", mainMenu);
        }

        private void DetermineCurrentItem(NavigationMenu menu)
        {
            INode currentNode = Node.GetCurrent();
            while (currentNode != null)
            {
                foreach (var item in menu.MenuItems)
                {
                    if (item.HyperLink.NodeId == currentNode.Id)
                    {
                        item.Current = true;
                        return;
                    }
                }
                currentNode = currentNode.Parent;
            }
        }
    }
}