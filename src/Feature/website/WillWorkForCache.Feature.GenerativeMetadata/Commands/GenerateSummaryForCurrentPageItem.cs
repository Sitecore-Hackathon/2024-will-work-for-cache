using System;
using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;
using Sitecore;


namespace WillWorkForCache.Feature.GenerativeMetadata.Commands
{
    public class GenerateSummaryForCurrentPageItem : TextAnalysisCommandBase
    {

        public override void Execute(CommandContext context)
        {
            var response = Context.ClientPage.ClientResponse;

            try
            {
                var item = GetItemOrThrowException(context);
                var keywords = GetSummaryForPageItem(item);

                SetContentEditorFieldValue(response, "Page description", keywords, false, false);
                SetContentEditorFieldValue(response, "Tweet description", keywords, false, false);
                SetContentEditorFieldValue(response, "Description", keywords, true, false);
            }
            catch (Exception e)
            {
                response.Alert(e.Message);
            }
        }

        public static string GetSummaryForPageItem(Item item)
        {
            var result = GetCachedTextAnalysisResult(item);

            var summary = result.Summary;
            if (string.IsNullOrWhiteSpace(summary))
                throw new Exception("Sorry, no summary could be generated for this page.");

            return summary;
        }
    }
}