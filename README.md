![Hackathon Logo](docs/images/hackathon.png?raw=true "Hackathon Logo")
# Sitecore Hackathon 2024

## Team name
⟹ Will Work For Cache

## Category
⟹ Best use of AI

## Description
Our module uses Azure AI services to help content authors improve the quality of the metadata within their site, rather than trying to generate net-new content.

This allows content authors to focus on the creation and curation of high-quality content while enabling automation of the often-overlooked boilerplate tasks that can keep the site accessible and searchable.

* Image alt tag generation using Azure AI Vision Studio
* Keyword and meta text summarization using Azure Text Analytics

The module is designed to be useful and usable across a wide variety of existing Sitecore projects, and to be easily extended for other use cases (e.g. to populate additional text fields).

## Video link

⟹ [Sitecore-Hackathon-2024-WillWorkForCache.mp4](Sitecore-Hackathon-2024-WillWorkForCache.mp4)

## Pre-requisites and Dependencies
* Sitecore 10.3.1 XM installed and running (containerized is fine) with a valid Sitecore license
* SXA + SPE
* Azure resources created, with endpoint URLs and access keys generated for:
    * 1x Computer Vision instance - for image alt tag generation - free tier is fine
    * 1x Text Analytics instance - Azure Cognitive Service for Language - for text summarization
 
_Note: An existing set of endpoint URLs and access keys will be emailed through for judging so no additional resources will need to be created._

## Installation instructions

1. This module runs on CM and requires two assembly binding redirects to be applied within the CM web.config. There is an updated copy of the base 10.3.1 XM web.config located here: [env/docker/deploy/cm/Web.config](env/docker/deploy/cm/Web.config) - copy this to your CM instance's wwwroot directory, or deploy it to the webroot of your CM docker instance.
    * The only changes in this file are the bindings for *Azure.Core* and *System.Diagnostics.DiagnosticSource* as part of a larger project, would be applied via a web.config transform as part of the build process.
2. Use the Sitecore Installation wizard to install the two packages:
    * [sitecore_packages/WillWorkForCache-GenerativeMetadata-Module-1.0.zip](sitecore_packages/WillWorkForCache-GenerativeMetadata-Module-1.0.zip)
    * [sitecore_packages/WillWorkForCache-Example-Content-Package-1.0.zip](sitecore_packages/WillWorkForCache-Example-Content-Package-1.0.zip)
6. Update the configuration item listed.

### Configuration

#### Required Settings

Update the following fields on the */sitecore/system/Settings/Feature/WillWorkForCache/GenerativeMetadata/GenerativeMetadataSettings* item - these will be blank by default.

_Note: An existing set of endpoint URLs and access keys will be emailed through for judging._

![Settings item](docs/images/Settings.png?raw=true "Settings item")

For the endpoint and access key for the Vision Services instance you want to use:
* **Vision Endpoint** should be the endpoint, e.g. "https://whfc2024-vision.cognitiveservices.azure.com/"
* **Vision Key** should be the access key, e.g. "0123456789abcdef0123456789abcdef"

For the endpoint and access key for the Text Analytics instance you want to use:
* **Language Endpoint** should be the endpoint, e.g. "https://hackathonlanguageanalysis.cognitiveservices.azure.com/"
* **Language Key** should be the access key, e.g. "0123456789abcdef0123456789abcdef"

If required, update the following setting in [/App_Config/Include/Feature/WillWorkForCache.Feature.GenerativeMetadata.config](src/Feature/website/WillWorkForCache.Feature.GenerativeMetadata/App_Config/Include/Feature/WillWorkForCache.Feature.GenerativeMetadata.config) - note that this is included as part of the installation package, so if no changes are required it will just be installed as-is.

* LOCALIZED_CM_URL should be set to the host name or IP address that can be used to retrieve content from the CM instance, i.e. by default, if running CM within a container, this will allow the CM server to download the contents of a rendered page from itself.

        <setting name="LOCALIZED_CM_URL" value="127.0.0.1" />

These are applied as Sitecore settings to take advantage of existing tooling - e.g. these values can easily be provided via environment variables in real containerized environments.

#### Optional Settings

The following settings in the .config file can be updated to configure the minimum confidence and maximum number of tags that will be saved for media items.

          <setting name="GenerativeMetadata.ImageTagMinimumConfidence" value="0.75" />
          <setting name="GenerativeMetadata.ImageTagMaximumCount" value="10" />

For example, changing these to "0.9" and "5" will only consider tags that the Vision Services instance has 90%+ confidence in, and it will only take a maximum of 5 tags when compiling the media item's list of valid keywords.

## Usage instructions

### Page Metadata

A new ribbon section named "Generative Metadata" is added to the "Review" tab.

![The buttons on the Review ribbon](docs/images/Text-Ribbon.png?raw=true "The buttons on the Review ribbon")

Thes options will be shown on all items but will only function on items that inherit from the */sitecore/templates/Foundation/Experience Accelerator/Multisite/Content/Page* template. Attempting to use these buttons on other items will display an alert informing the author that it is not supported.

The **Keywords** button will write to:
* Page Meta Properties > Page keywords

The **Summary** button will write to:
* Page Meta Properties > Page description
* OpenGraph > Description
* Twitter > Tweet description

![Sample fields that each button writes to](docs/images/Text-Generative.png?raw=true "Sample fields that each button writes to")

### Media Metadata

A new ribbon section named "Generative Metadata" is added to the "Media" tab for images. This is available whether viewing the Content Editor or the Media Library.

![The buttons on the Media ribbon](docs/images/Media-Ribbons.png?raw=true "The buttons on the Media ribbon")

This ribbon section contains three buttons which will pass the image to your Vision Services instance and then update the text in the relevant field with the content from the image analysis.

Each button will scroll to the field to show any changes being made so the content author can review or update before choosing to Save the item.

The API response is cached to reduce the number of API calls required where possible (e.g. between the first two buttons, and the third button if it is also in English). Only the response for one item at a time is cached to prevent cached data from persisting for too long.

If the API returns the same response as is currently in the field, a browser alert will be shown to confirm that nothing has changed.

**Alt Text** will update the "Alt" field. This will always return English content as the Azure image AI currently only supports caption generation in English.
![The Alt text field updated with generated English content](docs/images/Media-Alt.png?raw=true "The Alt text field updated with generated content")

**Keyword Tags** will update the "Keywords" field. Note that even if a different language version is selected, this button will always populate with the English content as well.
![The Keywords field updated with generated English content](docs/images/Media-Layers-EN.png?raw=true "The Keywords field updated with generated English content")

**Tags w/ Lang** will update the "Keywords" field with keyword tags generated using the language of the current item version. For example, after selecting Thai (a supported language):
![The Keywords field updated with generated Thai content](docs/images/Media-Layers.png?raw=true "The Keywords field updated with generated Thai content")
Not all languages are supported, so for example Albanian will return an error:
![The Keywords field cannot be updated with Albanian content](docs/images/Media-Layers-SQ.png?raw=true "The Keywords field cannot be updated with Albanian content")
 
## Comments

Built with a view for future extension:

* Experience Editor integration to allow for these fields to be populated from the EE view when working with an image or page
* Making it as easy as possible to write text in to a variety of other fields, e.g. when content authors are using other page templates with different field breakdowns
* Integration with Powershell reports - the current "Images with missing Alt text" report can be used to cycle through images and click the 'Alt Text' ribbon button, but tighter integration would be nice

## Sitecore Package Details

### WillWorkForCache-GenerativeMetadata-Module-1.0

This package contains the core functionality of this module.

Items
* /sitecore/content/Applications/Content Editor/Ribbons/Contextual Ribbons/Images/Media/Generative Metadata
* /sitecore/content/Applications/Content Editor/Ribbons/Chunks/Generative Metadata
* /sitecore/content/Applications/Content Editor/Ribbons/Strips/Review/Generative Metadata
* /sitecore/system/Settings/Feature/WillWorkForCache
* /sitecore/templates/Feature/WillWorkForCache

Files
* \App_Config\Include\Feature\WillWorkForCache.Feature.GenerativeMetadata.config
* \bin\Azure.AI.TextAnalytics.dll
* \bin\Azure.AI.Vision.ImageAnalysis.dll
* \bin\Azure.Core.dll
* \bin\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll
* \bin\System.ClientModel.dll
* \bin\System.Memory.Data.dll
* \bin\WillWorkForCache.Feature.GenerativeMetadata.dll
* \sitecore modules\GenerativeMetadata\Scripts\Assistant.js

### WillWorkForCache-Example-Content-Package-1.0

This package contains some sample pages and images that alt text / keywords / summaries can be generated from.

Items
* /sitecore/content/Hackathon
* /sitecore/Forms/Hackathon
* /sitecore/media library/Project/Hackathon
* /sitecore/media library/Themes/Hackathon
* /sitecore/templates/Project/Hackathon
* /sitecore/system/Settings/Foundation/Experience Accelerator/Multisite/Management/Sites
