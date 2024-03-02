using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Feeds.Sections;
using Sitecore.Shell.Framework.Commands;

namespace Hackathon.Feature.Commands
{
    public class GenerateKeywordTagsForCurrentMediaItem : ImageAnalysisCommandBase
    {
        public override void Execute(CommandContext context)
        {
            var response = Sitecore.Context.ClientPage.ClientResponse;
            string customLanguageCode = null;

            try
            {
                var item = GetItemOrThrowException(context);

                var isAutoLang = context.Parameters.Get("autolang") == "1";

                if (isAutoLang)
                {
                    customLanguageCode = item.Language.CultureInfo.TwoLetterISOLanguageName;
                }

                var tags = GetTagsForMediaItem(item, customLanguageCode);

                var matchingTags = FilterTags(tags);

                if (matchingTags.Count == 0)
                {
                    response.Alert($"{tags.Count} potential keyword tags were identified but none matched the minimum confidence criteria.\r\n\r\nThe disregarded keywords were: {string.Join(", ", tags)}");
                }
                else
                {
                    SetContentEditorFieldValue(response, "Keywords", string.Join(", ", matchingTags), true);
                }
            }
            catch (Exception e)
            {
                if (customLanguageCode != null && !string.Equals("en", customLanguageCode, StringComparison.OrdinalIgnoreCase))
                {
                    response.Alert("Sorry, it looks like tags are not yet supported for this language: " + customLanguageCode);
                }
                else
                {
                    response.Alert(e.Message);
                }
            }
        }

        public static IReadOnlyList<string> FilterTags(IReadOnlyList<DetectedTag> tags)
        {
            const string minimumConfidenceConfigKey = "WillCodeForCache.ImageTagMinimumConfidence";
            const string maximumCountConfigKey = "WillCodeForCache.ImageTagMaximumCount";

            Assert.ArgumentNotNull(tags, nameof(tags));

            var minimumConfidence = Settings.GetDoubleSetting(minimumConfidenceConfigKey, 0);
            if (minimumConfidence <= 0 || minimumConfidence > 1)
            {
                Log.Info($"{nameof(GenerateAltTextForCurrentMediaItem)}.{nameof(GetTagsForMediaItem)}: {minimumConfidenceConfigKey} has an invalid value, should be between 0 and 1, was: {minimumConfidence}", typeof(GenerateAltTextForCurrentMediaItem));
                return Array.Empty<string>();
            }

            var maximumCount = Settings.GetIntSetting(maximumCountConfigKey, 10);
            if (maximumCount < 1)
            {
                Log.Info($"{nameof(GenerateAltTextForCurrentMediaItem)}.{nameof(GetTagsForMediaItem)}: {maximumCountConfigKey} has an invalid value, should be greater than 0, was: {maximumCount}", typeof(GenerateAltTextForCurrentMediaItem));
                return Array.Empty<string>();
            }

            return tags
                .Where(tag => tag.Confidence >= minimumConfidence)
                .Take(maximumCount)
                .Select(tag => tag.Name)
                .ToList();
        }

        public static IReadOnlyList<DetectedTag> GetTagsForMediaItem(Item item, string optionalLanguageCode)
        {
            var features = string.IsNullOrWhiteSpace(optionalLanguageCode)
                ? VisualFeatures.Caption | VisualFeatures.Tags
                : VisualFeatures.Tags;

            var result = GetCachedImageAnalysisResult(item, features, optionalLanguageCode);

            var tags = result.Tags?.Values;
            if(tags == null || tags.Count == 0)
                throw new Exception("Sorry, no tags could be generated for this image.");

            return result.Tags.Values;
       }
    }
}