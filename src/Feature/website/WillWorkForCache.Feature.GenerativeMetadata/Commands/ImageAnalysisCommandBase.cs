﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Azure.AI.Vision.ImageAnalysis;
using Azure;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace WillWorkForCache.Feature.GenerativeMetadata.Commands
{
    public abstract class ImageAnalysisCommandBase : AnalysisCommandBase
    {
        private class CachedImageAnalysisResult
        {
            /// <summary>
            /// The ID of the item that was analyzed to generate this result.
            /// </summary>
            public Guid ItemId { get; set; }

            public VisualFeatures Features { get; set; }

            /// <summary>
            /// The full API response for the analysis of this item.
            /// </summary>
            public ImageAnalysisResult Result { get; set; }
        }

        /// <summary>
        /// Returns the ImageAnalysisResult response for the image attached to the supplied item, or
        /// throws an exception if any of the required settings or item properties are invalid.
        ///
        /// This will cache the return result and continue to use that value until the user analyzes a different media item.
        /// </summary>
        /// <param name="item">The item to perform the image analysis on.</param>
        /// <param name="features">The features to request from the analysis service.</param>
        /// <param name="optionalLanguageCode"></param>
        public static ImageAnalysisResult GetCachedImageAnalysisResult(Item item, VisualFeatures features, string optionalLanguageCode)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            // Cache based on the current username and the name of this method so we don't have to manage multiple cache keys.
            var cacheKey = Sitecore.Context.GetUserName() + "_" + nameof(GetCachedImageAnalysisResult) + "_" + optionalLanguageCode;

            // Cache the analysis result alongside the item ID, so we will store the result until this user analyzes a different item.
            if (HttpRuntime.Cache[cacheKey] is CachedImageAnalysisResult cachedValue && cachedValue.ItemId == item.ID.Guid && (cachedValue.Features & features) == features)
                return cachedValue.Result;

            var value = new CachedImageAnalysisResult
            {
                ItemId = item.ID.Guid,
                Features = features,
                Result = GetImageAnalysisResult(item, features, optionalLanguageCode),
            };

            HttpRuntime.Cache[cacheKey] = value;

            return value.Result;
        }

        /// <summary>
        /// Returns the ImageAnalysisResult response for the image attached to the supplied item, or
        /// throws an exception if any of the required settings or item properties are invalid.
        /// </summary>
        public static ImageAnalysisResult GetImageAnalysisResult(Item item, VisualFeatures features, string optionalLanguageCode)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var media = Sitecore.Resources.Media.MediaManager.GetMedia(item);
            if (media == null)
                throw new Exception($"Unable to retrieve media for item: {item.ID}");

            if (!media.MediaData.HasContent)
                throw new Exception($"There is no media data present for item: {item.ID}");

            var settingsItem = GetGenerativeMetadataSettingsItem(item.Database);
            if (settingsItem == null)
                throw new Exception($"Unable to find settings for Generative Metadata: {item.ID}");

            var visionEndpoint = settingsItem["VisionEndpoint"];
            if (string.IsNullOrWhiteSpace(visionEndpoint))
                throw new Exception($"The VisionEndpoint setting did not have a value");

            var visionKey = settingsItem["VisionKey"];
            if (string.IsNullOrWhiteSpace(visionKey))
                throw new Exception($"The VisionKey setting did not have a value");

            try
            {
                var client = new ImageAnalysisClient(new Uri(visionEndpoint), new AzureKeyCredential(visionKey));

                byte[] buffer;
                using (var mediaStream = media.GetStream())
                {
                    buffer = new byte[mediaStream.Length];
                    mediaStream.Stream.Read(buffer, 0, buffer.Length);
                }

                ImageAnalysisOptions options = string.IsNullOrEmpty(optionalLanguageCode) ? default : new ImageAnalysisOptions { Language = optionalLanguageCode };

                ImageAnalysisResult result = client.Analyze(new BinaryData(buffer), features, options);

                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Sorry, an error occurred trying to generate a caption for this image.\r\n\r\n" + e.Message, e);
            }
        }
    }
}