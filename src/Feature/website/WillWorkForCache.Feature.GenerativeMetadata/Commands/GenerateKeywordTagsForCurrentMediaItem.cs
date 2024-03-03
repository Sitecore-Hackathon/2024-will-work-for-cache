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

namespace WillWorkForCache.Feature.GenerativeMetadata.Commands
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

                // The only custom parameter we care about here is if "autolang=1". If so, try and retrieve tags
                // by passing through the current item's two-letter ISO language code - however as English is the default
                // don't pass through a code for that to improve cacheability, since we can batch Caption+Tags only
                // in English / no specified language at the moment.
                var isAutoLang = context.Parameters.Get("autolang") == "1";
                if (isAutoLang)
                {
                    var languageCode = item.Language.CultureInfo.TwoLetterISOLanguageName;
                    customLanguageCode = string.Equals("en", languageCode, StringComparison.OrdinalIgnoreCase) ? null : languageCode;
                }

                var tags = GetTagsForMediaItem(item, customLanguageCode);

                var matchingTags = FilterTags(tags);

                if (matchingTags.Count == 0)
                {
                    response.Alert($"{tags.Count} potential keyword tags were identified but none matched the minimum confidence criteria.\r\nThe disregarded keywords were: {string.Join(", ", tags.Select(t => $"{t.Name} ({t.Confidence:F2})"))}");
                }
                else
                {
                    SetContentEditorFieldValue(response, "Keywords", string.Join(", ", matchingTags), true, true);
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
            const string minimumConfidenceConfigKey = "GenerativeMetadata.ImageTagMinimumConfidence";
            const string maximumCountConfigKey = "GenerativeMetadata.ImageTagMaximumCount";

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
                .Where(tag => (tag.Confidence - minimumConfidence) > -0.01)
                .Take(maximumCount)
                .Select(tag => tag.Name)
                .ToList();
        }

        public static IReadOnlyList<DetectedTag> GetTagsForMediaItem(Item item, string optionalLanguageCode)
        {
            // The vision API currently only supports generating the caption in English (which is the default if no language
            // parameter is supplied). Making the call with caching enabled, with no language selected, and requesting
            // Caption + Tags will mean that the response is as reusable by other commands as possible.
            // What that means here is if we aren't requesting a specific language -> get Caption + Tags; otherwise just get Tags.
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