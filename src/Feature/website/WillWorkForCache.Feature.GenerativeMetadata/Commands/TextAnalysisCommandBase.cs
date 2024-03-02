using HtmlAgilityPack;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Web;
using Sitecore.Configuration;
using Sitecore.XA.Foundation.Multisite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WillWorkForCache.Feature.GenerativeMetadata.Models;
using Azure.AI.TextAnalytics;
using Azure;

namespace WillWorkForCache.Feature.GenerativeMetadata.Commands
{
    public abstract class TextAnalysisCommandBase : AnalysisCommandBase
    {
        static string languageKey => Settings.GetSetting("LANGUAGE_KEY");
        static string languageEndpoint => Settings.GetSetting("LANGUAGE_ENDPOINT");
        static string localizedCMurl => Settings.GetSetting("LOCALIZED_CM_URL");

        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
        private static readonly Uri endpoint = new Uri(languageEndpoint);

        private class CachedTextAnalysisResult
        {
            /// <summary>
            /// The ID of the item that was analyzed to generate this result.
            /// </summary>
            public Guid ItemId { get; set; }
            
            /// <summary>
            /// The full API response for the analysis of this item.
            /// </summary>
            public AnalyzedPageResult Result { get; set; }
        }

        public static AnalyzedPageResult GetCachedTextAnalysisResult(Item item, string optionalLanguageCode)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            // Cache based on the current username and the name of this method so we don't have to manage multiple cache keys.
            var cacheKey = Sitecore.Context.GetUserName() + "_" + nameof(GetCachedTextAnalysisResult) + "_" + optionalLanguageCode;

            // Cache the analysis result alongside the item ID, so we will store the result until this user analyzes a different item.
            if (HttpRuntime.Cache[cacheKey] is CachedTextAnalysisResult cachedValue && cachedValue.ItemId == item.ID.Guid)
                return cachedValue.Result;

            var value = new CachedTextAnalysisResult
            {
                ItemId = item.ID.Guid,
                Result = GetTextAnalysisResult(item, optionalLanguageCode),
            };

            HttpRuntime.Cache[cacheKey] = value;

            return value.Result;
        }

        /// <summary>
        /// Get page content from the provided Item and parse the text from the main element of the page
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetParsedPageContentForSummary(Item item)
        {   
            // exit if key and endpoint are not configured
            if (string.IsNullOrEmpty(languageKey) || string.IsNullOrEmpty(languageEndpoint) || string.IsNullOrEmpty(localizedCMurl))
                throw new Exception($"Unable to generate summaries for item: {item.ID}");

            // check if item is a page item inheriting from SXA Page base template
            if (!TemplateManager.GetTemplate(item).InheritsFrom(new ID(Constants.PageBaseTemplate.TemplateId)))
                throw new Exception($"Item is not a page and cannot have a summary generated: {item.ID}");

            // get site info from page item to make sure item is within a site
            string siteName = GetSiteInfoFromPath(item);
            if (string.IsNullOrEmpty(siteName))
                throw new Exception($"Item is not within a site and cannot be rendered to generate a summary: {item.ID}");

            // get url from localized setting and site querystring param to handle multisite
            string original = LinkManager.GetItemUrl(item);
            var itemPageUrl = GetLocalUrl(original, localizedCMurl, siteName);

            // execute web request to get rendered html for the page
            var pageString = WebUtil.ExecuteWebPage(itemPageUrl);

            // parse text content out of html from rendered page
            var parsedPageContent = GetTrimmedHTML(pageString);

            // check if there is content within the page 
            if (string.IsNullOrEmpty(parsedPageContent))
                throw new Exception($"There is no content on the page so a summary cannot be generated: {item.ID}");            

            return parsedPageContent;
        }

        public static AnalyzedPageResult GetTextAnalysisResult(Item item, string optionalLanguageCode)
        {
            var parsedPageContent = GetParsedPageContentForSummary(item);

            var clientOptions = new TextAnalyticsClientOptions
            {
                DefaultLanguage = item.Language.ToString()
            };
            var client = new TextAnalyticsClient(endpoint, credentials, clientOptions);

            // Perform the text analysis operation.
            var pageSummary = GetSummarization(client, parsedPageContent);
            var pageKeywords = GetKeywords(client, parsedPageContent);

            return new AnalyzedPageResult
            {
                Summary = pageSummary,
                Keywords = pageKeywords
            };
        }

        /// <summary>
        /// Get summary of the contents of the page content from the Azure Summarization API
        /// </summary>
        /// <param name="client">The initialized client object making the request to the API </param>
        /// <param name="pageContent">Parsed text content from the context page to summarize</param>
        /// <returns></returns>
        private static string GetSummarization(TextAnalyticsClient client, string pageContent)
        {
            // Perform the text analysis operation.
            AbstractiveSummarizeOperation operation = client.AbstractiveSummarize(WaitUntil.Completed,
                new List<string>()
                {
                    pageContent
                });

            //get summary from the Azure API and return the first valid summary text
            foreach (AbstractiveSummarizeResultCollection documentsInPage in operation.GetValues())
            {
                foreach (AbstractiveSummarizeResult documentResult in documentsInPage)
                {
                    if (documentResult.HasError)
                        throw new Exception($"Document Error - Message: {documentResult.Error.Message}");                    

                    if (documentResult.Summaries.Any())
                    {
                        return documentResult.Summaries.First().Text;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Extract keywords from the contents of the page content using the Azure Summarization API
        /// </summary>
        /// <param name="client">The initialized client object making the request to the API</param>
        /// <param name="pageContent">Parsed text content from the context page to extract from</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static string GetKeywords(TextAnalyticsClient client, string pageContent)
        {
            Response<KeyPhraseCollection> response = client.ExtractKeyPhrases(pageContent);
            KeyPhraseCollection keyPhrases = response.Value;

            if (!keyPhrases.Any())
                throw new Exception($"Unable to generate keywords.");

            //replace double spaces with empty strings to trim interior whitespace returned by the API
            return string.Join(",", keyPhrases).Replace("  ", "");
        }

        /// <summary>
        /// Get main tag from html and parse all text from within the html tags
        /// </summary>
        /// <param name="fullHtml">The html from the page to be parsed</param>
        /// <returns></returns>
        protected static string GetTrimmedHTML(string fullHtml)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(fullHtml);

            StringBuilder parsedText = new StringBuilder();

            var mainNode = doc.DocumentNode.SelectSingleNode("//body//main");
            if (mainNode != null)
            {
                foreach (HtmlNode node in mainNode.SelectNodes("//text()"))
                {
                    parsedText.AppendLine(node.InnerText);
                }
            }

            return parsedText.ToString();
        }

        /// <summary>
        /// Replace hostname in url
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newHostName"></param>
        /// <returns></returns>
        protected static string GetLocalUrl(string original, string newHostName, string siteName)
        {
            var builder = new UriBuilder(original);
            builder.Host = newHostName;
            builder.Scheme = Uri.UriSchemeHttp;
            builder.Port = -1;
            return builder.Uri.ToString() + "?sc_site=" + siteName;
        }

        protected static string GetSiteInfoFromPath(Item item)
        {
            SiteInfoResolver resolver = new SiteInfoResolver();
            SiteInfo parentSite = resolver.GetSiteInfo(item);

            return parentSite?.Name ?? string.Empty;
        }
    }
}