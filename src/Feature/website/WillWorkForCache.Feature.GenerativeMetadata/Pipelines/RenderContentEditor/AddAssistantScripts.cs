using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.StringExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Sitecore.Configuration;
using Sitecore.Resources;

namespace WillWorkForCache.Feature.GenerativeMetadata.Pipelines.RenderContentEditor
{
    public class AddAssistantScripts
    {
        // Inspired by https://jammykam.wordpress.com/2014/04/24/adding-custom-javascript-and-stylesheets-in-the-content-editor/

        public void Process(PipelineArgs args)
        {
            var resources = Settings.GetSetting("GenerativeMetadata.ScriptFiles");

            if (string.IsNullOrEmpty(resources))
                return;

            var builder = new StringBuilder();
            builder.AppendLine($"<!-- Start {nameof(AddAssistantScripts)} scripts -->");

            foreach (var resource in resources.Split('|'))
            {
                builder.AppendFormat("<script type=\"text/javascript\" src=\"{0}\"></script>\r\n", resource);
            }

            builder.AppendLine($"<!-- End {nameof(AddAssistantScripts)} scripts -->");

            Sitecore.Context.Page.Page.Header.Controls.Add(new LiteralControl(builder.ToString()));

        }
    }
}