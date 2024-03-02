using System;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Sitecore.Configuration;
using Sitecore.Shell.Framework.Commands;

namespace Hackathon.Feature.Commands
{
    public class GenerateAltTextForCurrentMediaItem : Command
    {
        public override void Execute(CommandContext context)
        {
            if (context.Items.Length != 1)
            {
                Sitecore.Context.ClientPage.ClientResponse.Alert("Unable to determine the current item");
                return;
            }

            var item = context.Items[0];

            var media = Sitecore.Resources.Media.MediaManager.GetMedia(item);
            if (media == null)
            {
                Sitecore.Context.ClientPage.ClientResponse.Alert($"Unable to retrieve media for item: {item.ID}");
                return;
            }

            var visionEndpoint = Settings.GetSetting("VISION_ENDPOINT");
            if (string.IsNullOrWhiteSpace(visionEndpoint))
            {
                Sitecore.Context.ClientPage.ClientResponse.Alert($"The VISION_ENDPOINT setting did not have a value");
                return;
            }

            var visionKey = Settings.GetSetting("VISION_KEY");
            if (string.IsNullOrWhiteSpace(visionKey))
            {
                Sitecore.Context.ClientPage.ClientResponse.Alert($"The VISION_KEY setting did not have a value");
                return;
            }

            // Read media stream to buffer
            byte[] buffer;
            using (var mediaStream = media.GetStream())
            {
                buffer = new byte[mediaStream.Length];
                mediaStream.Stream.Read(buffer, 0, buffer.Length);
            }

            try
            {
                ImageAnalysisClient client = new ImageAnalysisClient(new Uri(visionEndpoint), new AzureKeyCredential(visionKey));

                ImageAnalysisResult result = client.Analyze(new BinaryData(buffer), VisualFeatures.Caption);

                Sitecore.Context.ClientPage.ClientResponse.Alert($"   '{result.Caption.Text}', Confidence {result.Caption.Confidence:F4}");
            }
            catch (Exception e)
            {
                Sitecore.Context.ClientPage.ClientResponse.ShowError(e);
                return;
            }
        }
    }
}