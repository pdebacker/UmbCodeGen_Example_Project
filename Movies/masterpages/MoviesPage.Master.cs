using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Movies.Helpers;
using Movies.Models;
using umbraco.interfaces;
using umbraco.NodeFactory;

namespace Movies.masterpages
{
    public partial class MoviesPage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.HttpMethod.Equals("POST"))
                PostRequest();
            else
                GetRequest();
        }

        private void GetRequest()
        {
            INode currentNode = Node.GetCurrent();
            var movies = ModelFactory.CreateModel<Movies.Models.Movies>(currentNode);
            pageContent.Text = MustacheHelper.RenderMustacheTemplate(this, "movies", movies);

            INode genresNode = Node.GetNodeByXpath("//Genres");
            var genres = ModelFactory.CreateModel<Genres>(genresNode);
            sidebarContent.Text = MustacheHelper.RenderMustacheTemplate(this, "sidebar", genres);
        }

        private void PostRequest()
        {
            int genreId = int.Parse(Request.QueryString["genreId"]);
            INode currentNode = Node.GetCurrent();
            var movies = ModelFactory.CreateModel<Movies.Models.Movies>(currentNode);

            movies.MovieListItems = movies.MovieListItems.Where(l => l.Genre.NodeId == genreId).ToList();

            movies.DonNotSerializeThis = "Foo-bar";

            Response.Write(new JavaScriptSerializer().Serialize(movies));
            Response.Flush();
            Response.End();
        }
    }
}