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


namespace Hackathon.Feature.Commands
{
    public class GenerateSummaryForCurrentPageItem : Command
    {
        // TODO: remove keys and use env variables
        static string languageKey = "cf708cb08fc146b18a22ff96015fd1ac";
        static string languageEndpoint = "https://hackathonlanguageanalysis.cognitiveservices.azure.com/";

        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
        private static readonly Uri endpoint = new Uri(languageEndpoint);

        public override void Execute(CommandContext context)
        {
            if (context.Items.Length != 1)
            {
                Sitecore.Context.ClientPage.ClientResponse.Alert("Unable to determine the current item");
                return;
            }

            // TODO: do something better with the url replacement - this will only work in containers, and with a single site
            string original = LinkManager.GetItemUrl(context.Items[0]);
            var itemPageUrl = GetLocalUrl(original, "127.0.0.1");

            var pageString = Sitecore.Web.WebUtil.ExecuteWebPage(itemPageUrl);

            //parse text content out of html from rendered page
            var parsedPageContent = GetTrimmedHTML(pageString);

            string pageSummary;
            try
            {
                var client = new TextAnalyticsClient(endpoint, credentials);

                // Perform the text analysis operation.
                pageSummary = GetSummarization(client, parsedPageContent);

            }
            catch (Exception e)
            {
                Sitecore.Context.ClientPage.ClientResponse.ShowError(e);
                return;
            }

            if (string.IsNullOrEmpty(pageSummary))
            {
                pageSummary = "Unable to generate summary.";
            }

            Context.ClientPage.ClientResponse.Alert(pageSummary);
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
        private string GetLocalUrl(string original, string newHostName)
        {
            var builder = new UriBuilder(original);
            builder.Host = newHostName;
            builder.Scheme = Uri.UriSchemeHttp;
            builder.Port = -1;
            return builder.Uri.ToString();
        }
    }
}