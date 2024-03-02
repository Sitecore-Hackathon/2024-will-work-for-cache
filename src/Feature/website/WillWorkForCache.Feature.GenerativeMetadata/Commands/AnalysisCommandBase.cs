using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using System;
using System.Web;

namespace WillWorkForCache.Feature.GenerativeMetadata.Commands
{
    public abstract class AnalysisCommandBase : Command
    {

        /// <summary>
        /// Returns the only item from context.Items, or throws an exception.
        /// </summary>
        protected Item GetItemOrThrowException(CommandContext context)
        {
            if (context.Items.Length != 1)
                throw new Exception("Unable to determine the current item");

            return context.Items[0];
        }

        /// <summary>
        /// Passes back Javascript to the supplied ClientResponse to update the value of a text field.
        /// </summary>
        /// <param name="response">The Sitecore.Context.ClientPage.ClientResponse to write this Javascript snippet to.</param>
        /// <param name="fieldTitle">The field title to match in the content editor, e.g. "Keywords" or "Alt"</param>
        /// <param name="newValue">The value to write in to this text input field</param>
        /// <param name="scrollToField">If true, try to scroll the browser to the targeted field.</param>
        /// <param name="alertIfSame">True to show an alert if the value is already the same, false to silently ignore.</param>
        protected static void SetContentEditorFieldValue(ClientResponse response, string fieldTitle, string newValue, bool scrollToField = true, bool alertIfSame = true)
        {
            response.Eval($"window.generativeMetadata.setFieldValueByName(\"{HttpUtility.JavaScriptStringEncode(fieldTitle)}\", \"{HttpUtility.JavaScriptStringEncode(newValue)}\", {(scrollToField ? "true" : "false")}, {(alertIfSame ? "true" : "false")});");
        }
    }
}