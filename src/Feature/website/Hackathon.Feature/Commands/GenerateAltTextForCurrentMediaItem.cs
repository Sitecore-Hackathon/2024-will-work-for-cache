using System;
using System.Web;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace Hackathon.Feature.Commands
{
    public class GenerateAltTextForCurrentMediaItem : Command
    {
        public override void Execute(CommandContext context)
        {
            var response = Sitecore.Context.ClientPage.ClientResponse;

            try
            {
                if (context.Items.Length != 1)
                    throw new Exception("Unable to determine the current item");

                var item = context.Items[0];

                var caption = GetAltTextForMediaItem(item);
                response.Eval($"window.willWorkForCache.setFieldValueByName(\"Alt\", \"{HttpUtility.JavaScriptStringEncode(caption)}\", true);");
            }
            catch (Exception e)
            {
                response.Alert(e.Message);
            }
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetAltTextForMediaItem(Item item)
        {
            var media = Sitecore.Resources.Media.MediaManager.GetMedia(item);
            if (media == null)
                throw new Exception($"Unable to retrieve media for item: {item.ID}");

            if (!media.MediaData.HasContent)
                throw new Exception($"There is no media data present for item: {item.ID}");

            var visionEndpoint = Settings.GetSetting("VISION_ENDPOINT");
            if (string.IsNullOrWhiteSpace(visionEndpoint))
                throw new Exception($"The VISION_ENDPOINT setting did not have a value");

            var visionKey = Settings.GetSetting("VISION_KEY");
            if (string.IsNullOrWhiteSpace(visionKey))
                throw new Exception($"The VISION_KEY setting did not have a value");

            try
            {
                var client = new ImageAnalysisClient(new Uri(visionEndpoint), new AzureKeyCredential(visionKey));

                byte[] buffer;
                using (var mediaStream = media.GetStream())
                {
                    buffer = new byte[mediaStream.Length];
                    mediaStream.Stream.Read(buffer, 0, buffer.Length);
                }

                // Image API can currently only produce the captions in EN - once this is expanded, can pass through a
                // new ImageAnalysisOptions { Language = Sitecore.Context.Language.CultureInfo.TwoLetterISOLanguageName }
                // (or switch based on the language code if only a subset is supported, etc)

                ImageAnalysisResult result = client.Analyze(new BinaryData(buffer), VisualFeatures.Caption);

                var caption = result.Caption.Text;
                if (string.IsNullOrWhiteSpace(caption))
                    throw new Exception("Sorry, no caption could be generated for this image.");

                // Caption comes back all lowercase, so uppercase the first character
                caption = caption.Substring(0, 1).ToUpper() + caption.Substring(1);

                return caption;
            }
            catch (Exception e)
            {
                throw new Exception("Sorry, an error occurred trying to generate a caption for this image.\r\n\r\n" + e.Message, e);
            }
        }
    }
}