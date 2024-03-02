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