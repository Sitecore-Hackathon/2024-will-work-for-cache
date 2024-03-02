using System;
using System.Web;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Sitecore.Configuration;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace Hackathon.Feature.Commands
{
    public class GenerateAltTextForCurrentMediaItem : Command
    {
        public override void Execute(CommandContext context)
        {
            var response = Sitecore.Context.ClientPage.ClientResponse;

            if (context.Items.Length != 1)
            {
                response.Alert("Unable to determine the current item");
                return;
            }

            var item = context.Items[0];

            var media = Sitecore.Resources.Media.MediaManager.GetMedia(item);
            if (media == null)
            {
                response.Alert($"Unable to retrieve media for item: {item.ID}");
                return;
            }

            var visionEndpoint = Settings.GetSetting("VISION_ENDPOINT");
            if (string.IsNullOrWhiteSpace(visionEndpoint))
            {
                response.Alert($"The VISION_ENDPOINT setting did not have a value");
                return;
            }

            var visionKey = Settings.GetSetting("VISION_KEY");
            if (string.IsNullOrWhiteSpace(visionKey))
            {
                response.Alert($"The VISION_KEY setting did not have a value");
                return;
            }

            try
            {
                var client = new ImageAnalysisClient(new Uri(visionEndpoint), new AzureKeyCredential(visionKey));

                byte[] buffer;
                using (var mediaStream = media.GetStream())
                {
                    buffer = new byte[mediaStream.Length];
                    mediaStream.Stream.Read(buffer, 0, buffer.Length);
                }

                ImageAnalysisResult result = client.Analyze(new BinaryData(buffer), VisualFeatures.Caption);

                var caption = result.Caption.Text;

                if (string.IsNullOrWhiteSpace(caption))
                {
                    response.Alert("Sorry, no caption could be generated for this image.");
                    return;
                }

                // Caption comes back all lowercase, so uppercase the first character
                caption = caption.Substring(0, 1).ToUpper() + caption.Substring(1);

                response.Eval($"window.willWorkForCache.setFieldValueByName(\"Alt\", \"{HttpUtility.JavaScriptStringEncode(caption)}\", true);");
            }
            catch (Exception e)
            {
                if (e is AggregateException agg)
                {
                    foreach (var ex in agg.InnerExceptions)
                    {
                        response.ShowError(ex);
                    }
                }
                else
                {
                    response.ShowError(e);
                }
            }
        }
    }
}