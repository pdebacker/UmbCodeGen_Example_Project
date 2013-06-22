using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.UI;


namespace Movies.Helpers
{
    public class MustacheHelper
    {
        public static string RenderMustacheTemplate(Control ctrl,  string templateId, object data)
        {
            var instance = new MustacheHelper();
            return instance.RenderMustacheTemplateInternal(ctrl, templateId, data);
        }

        private string RenderMustacheTemplateInternal(Control ctrl, string templateId, object data)
        {
            if (ctrl == null)
                throw new ArgumentException("(Parent) control  expected", "ctrl");

            if (string.IsNullOrEmpty(templateId))
                throw new ArgumentException("Literal control id expected", "templateId");

            control = ctrl;
            var template = LoadMustacheTemplate(templateId);
            if (template != null)
            {
                StringWriter writer = new StringWriter();
                template.Render(data, writer, LoadMustacheTemplate);
                return writer.ToString();
            }
            return null;
        }


        private  Nustache.Core.Template LoadMustacheTemplate(string templateId)
        {
            templateId = templateId.Trim();
            if (templateId.EndsWith(".mustache"))
            {
                templateId = templateId.Remove(templateId.Length - ".mustache".Length);
            }
            string controlId = string.Format("{0}MustacheTemplate", templateId);
            var literal = control.FindControlRecursive(controlId) as Literal;
            if (literal != null)
            {
                string mustacheTemplateText = literal.Text;
                TextReader reader = new StringReader(mustacheTemplateText);

                var template = new Nustache.Core.Template(templateId);
                template.Load(reader);
                return template;
            }
            return null;
        }

        private  Control control;

    }
}