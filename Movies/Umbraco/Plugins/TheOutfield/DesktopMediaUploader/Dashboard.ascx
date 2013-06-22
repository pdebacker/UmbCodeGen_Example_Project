<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="TheOutfield.UmbExt.DesktopMediaUploader.Web.UserControls.DesktopMediaUploader.Dashbaord" %>
<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/swfobject/2.1/swfobject.js"></script>
<div class="propertyDiv" style="text-align: center;margin-top: 10px;">
    <p><strong>Desktop Media Uploader</strong> is a small desktop application that you can install on your computer which allows you to easily upload media items directly to the media section.</p>
    <p>The badge below will auto configure itself based upon whether you already have <strong>Desktop Media Uploader</strong> installed or not.</p>
    <p>Just click the <strong>Install Now / Upgrade Now / Launch Now</strong> link to perform that action.</p>
    <p>
        <div id="flashcontent" style="display:block;margin-bottom: 10px;">
            Download <a href="<%= FullyQualifiedAppPath %>umbraco/plugins/theoutfield/desktopmediauploader/desktopmediauploader.air">Desktop Media Uploader</a> now.<br /><br /><span id="AIRDownloadMessageRuntime">This application requires Adobe&#174;&nbsp;AIR&#8482; to be installed for <a href="http://airdownload.adobe.com/air/mac/download/latest/AdobeAIR.dmg">Mac OS</a> or <a href="http://airdownload.adobe.com/air/win/download/latest/AdobeAIRInstaller.exe">Windows</a>.
        </div>
    </p>
    <script type="text/javascript">
    // <![CDATA[

        var flashvars = {
            appid: "com.theoutfield.DesktopMediaUploader",
            appname: "Desktop Media Uploader",
            appversion: "v2.0.5",
            appurl: "<%= FullyQualifiedAppPath %>umbraco/plugins/theoutfield/desktopmediauploader/desktopmediauploader.air",
            applauncharg: "<%= AppLaunchArg %>",
            image: "/umbraco/plugins/theoutfield/desktopmediauploader/badge.jpg?v=2.0.5",
            airversion: "2.0"
        };
        var params = {
            menu: "false",
	    wmode: "opaque"
        };
        var attributes = {
            style: "margin-bottom:10px;"
        };

        swfobject.embedSWF("/umbraco/plugins/theoutfield/desktopmediauploader/airinstallbadge.swf", "flashcontent", "215", "180", "9.0.115", "/umbraco/plugins/theoutfield/desktopmediauploader/expressinstall.swf", flashvars, params, attributes);

    // ]]>
    </script>
    <p>For a quick guide on how to use the <strong>Desktop Media Uploader</strong>, <a href="http://screenr.com/vXr" target="_blank">checkout this video</a>.</p>
</div>