using System;
using Azure.AI.TextAnalytics;
using Azure;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Links;
using Sitecore;
using HtmlAgilityPack;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Web;
using Sitecore.XA.Foundation.Multisite;
using Sitecore.Data.Managers;
using Sitecore.Data;
using System.Web;


namespace WillWorkForCache.Feature.GenerativeMetadata.Commands
{
    public class GenerateSummaryForCurrentPageItem : Command
    {

        static string languageKey => Settings.GetSetting("LANGUAGE_KEY");
        static string languageEndpoint => Settings.GetSetting("LANGUAGE_ENDPOINT");
        static string localizedCMurl => Settings.GetSetting("LOCALIZED_CM_URL");

        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
        private static readonly Uri endpoint = new Uri(languageEndpoint);

        public override void Execute(CommandContext context)
        {
            var response = Context.ClientPage.ClientResponse;

            if (context.Items.Length != 1)
            {
                response.Alert("Unable to determine the current item.");
                return;
            }

            //exit if key and endpoint are not configured
            if (string.IsNullOrEmpty(languageKey) || string.IsNullOrEmpty(languageEndpoint) || string.IsNullOrEmpty(localizedCMurl))
            {
                response.Alert("Unable to generate summaries.");
                return;
            }

            var contextItem = context.Items[0];
            //if item is not a page item, return
            if (!TemplateManager.GetTemplate(contextItem).InheritsFrom(new ID(Constants.PageBaseTemplate.TemplateId)))
            {
                response.Alert("Item is not a page and cannot have a summary generated.");
                return;
            }

            // get site info from page item to make sure item is within a site and to append to the url to handle multisite
            string siteName = GetSiteInfoFromPath(contextItem);
            if (string.IsNullOrEmpty(siteName))
            {
                response.Alert("Item is not within a site and cannot be rendered to generate a summary.");
                return;
            }

            //get url from localized setting and site querystring param
            string original = LinkManager.GetItemUrl(contextItem);
            var itemPageUrl = GetLocalUrl(original, localizedCMurl, siteName);

            var pageString = WebUtil.ExecuteWebPage(itemPageUrl);

            //parse text content out of html from rendered page
            var parsedPageContent = GetTrimmedHTML(pageString);

            if (string.IsNullOrEmpty(parsedPageContent))
            {
                response.Alert("There is no content on the page so a summary cannot be generated.");
                return;
            }

            string pageSummary;
            try
            {
                var clientOptions = new TextAnalyticsClientOptions
                {
                    DefaultLanguage = contextItem.Language.ToString()
                };
                var client = new TextAnalyticsClient(endpoint, credentials, clientOptions);

                // Perform the text analysis operation.
                pageSummary = GetSummarization(client, parsedPageContent);

            }
            catch (Exception e)
            {
                response.ShowError(e);
                return;
            }

            if (string.IsNullOrEmpty(pageSummary))
            {
                response.Alert("Unable to generate summary.");
                return;
            }

            response.Eval($"window.generativeMetadata.setFieldValueByName(\"Page description\", \"{HttpUtility.JavaScriptStringEncode(pageSummary)}\", false, false);");
        }

        /// <summary>
        /// Get summary of the contents of the page content from the Azure Summarization API
        /// </summary>
        /// <param name="client"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        private string GetSummarization(TextAnalyticsClient client, string pageContent)
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
                    {
                        Log.Error($"  Error!", this);
                        Log.Error($"  Document error code: {documentResult.Error.ErrorCode}", this);
                        Log.Error($"  Message: {documentResult.Error.Message}", this);
                        continue;
                    }

                    if (documentResult.Summaries.Any())
                    {
                        return documentResult.Summaries.First().Text;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Get main tag from html and parse all text from within the html tags
        /// </summary>
        /// <param name="fullHtml"></param>
        /// <returns></returns>
        private string GetTrimmedHTML(string fullHtml)
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
        private string GetLocalUrl(string original, string newHostName, string siteName)
        {
            var builder = new UriBuilder(original);
            builder.Host = newHostName;
            builder.Scheme = Uri.UriSchemeHttp;
            builder.Port = -1;
            return builder.Uri.ToString() + "?sc_site=" + siteName;
        }

        private string GetSiteInfoFromPath(Sitecore.Data.Items.Item item)
        {
            SiteInfoResolver resolver = new SiteInfoResolver();
            SiteInfo parentSite = resolver.GetSiteInfo(item);

            return parentSite?.Name ?? string.Empty;
        }
    }
}