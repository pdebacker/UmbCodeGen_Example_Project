using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Movies.Helpers;
using Movies.Models;
using Movies.Classes;
using umbraco.interfaces;
using umbraco.NodeFactory;

namespace Movies.masterpages
{
    public partial class MoviePage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            INode currentNode = Node.GetCurrent();
            var movie = ModelFactory.CreateModel<Movie>(currentNode);
            FormatCast(movie);
            movie.HasRelatedMovieItems = movie.RelatedMovieItems.Count > 0;
            pageContent.Text = MustacheHelper.RenderMustacheTemplate(this, "movie", movie);

            var showDictionary = ShowDictionary.MovieShowingAt(movie);
            sidebarContent.Text = MustacheHelper.RenderMustacheTemplate(this, "sidebar", showDictionary);

        }

        private void FormatCast(Movie movie)
        {
            movie.Cast = movie.Cast.Replace("\n", ", ");
        }
            
    }

  
}