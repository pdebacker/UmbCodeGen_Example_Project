<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainNavigation.ascx.cs" Inherits="Movies.usercontrols.Navigation.MainNavigation" %>

<asp:literal id="topNavigationContent" runat="server" Visible="true"></asp:literal>

<script type="text/template" id="topNavigationMustacheTemplate" data-template="topNavigation.mustache">
    <asp:Literal ID="topNavigationMustacheTemplate" runat="server">
        <ul id="topNavigation">
        {{#MenuItems}}
            <li {{#Current}}class="current"{{/Current}}><a class="navigation" href="{{HyperLink.Url}}"><span>{{NodeName}}</span></a></li>
        {{/MenuItems}}
        </ul>
    </asp:Literal>        
</script>