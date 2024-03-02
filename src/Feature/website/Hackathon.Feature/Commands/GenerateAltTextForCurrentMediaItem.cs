using System;
using Azure.AI.Vision.ImageAnalysis;
using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;

namespace Hackathon.Feature.Commands
{
    public class GenerateAltTextForCurrentMediaItem : ImageAnalysisCommandBase
    {
        public override void Execute(CommandContext context)
        {
            var response = Sitecore.Context.ClientPage.ClientResponse;

            try
            {
                var item = GetItemOrThrowException(context);
                var caption = GetAltTextForMediaItem(item);

                SetContentEditorFieldValue(response, "Alt", caption, true, true);
            }
            catch (Exception e)
            {
                response.Alert(e.Message);
            }
        }

        public static string GetAltTextForMediaItem(Item item)
        {
            // The vision API currently only supports generating the caption in English (which is the default if no language
            // parameter is supplied). Making the call with caching enabled, with no language selected, and requesting
            // Caption + Tags will mean that the response is as reusable by other commands as possible.
            var result = GetCachedImageAnalysisResult(item, VisualFeatures.Caption | VisualFeatures.Tags, null);

            var caption = result.Caption.Text;
            if (string.IsNullOrWhiteSpace(caption))
                throw new Exception("Sorry, no caption could be generated for this image.");

            // Caption comes back all lowercase, so uppercase the first character
            caption = caption.Substring(0, 1).ToUpper() + caption.Substring(1);

            return caption;
        }
    }
}