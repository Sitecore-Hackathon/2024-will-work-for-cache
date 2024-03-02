using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;
using System;

namespace WillWorkForCache.Feature.GenerativeMetadata.Commands
{
    public class GenerateKeywordsForCurrentPageItem : TextAnalysisCommandBase
    {
        public override void Execute(CommandContext context)
        {
            var response = Sitecore.Context.ClientPage.ClientResponse;

            try
            {
                var item = GetItemOrThrowException(context);
                var keywords = GetKeywordsForPageItem(item);

                SetContentEditorFieldValue(response, "Page keywords", keywords, true, true);
            }
            catch (Exception e)
            {
                response.Alert(e.Message);
            }
        }

        public static string GetKeywordsForPageItem(Item item)
        {   
            var result = GetCachedTextAnalysisResult(item);

            var keywords = result.Keywords;
            if (string.IsNullOrWhiteSpace(keywords))
                throw new Exception("Sorry, no keywords could be generated for this page.");

            return keywords;
        }
    }
}