using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Movies.Helpers
{
    public static class ControlExtensions
    {
        public static Control FindControlRecursive(this Control control, string id)
        {
            if (control == null) return null;

            Control ctrl = control.FindControl(id);
            if (ctrl == null)
            {
                foreach (Control child in control.Controls)
                {
                    ctrl = FindControlRecursive(child, id);
                    if (ctrl != null) break;
                }
            }
            return ctrl;
        }
    }
}