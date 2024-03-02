![Hackathon Logo](docs/images/hackathon.png?raw=true "Hackathon Logo")
# Sitecore Hackathon 2024

- MUST READ: **[Submission requirements](SUBMISSION_REQUIREMENTS.md)**
- [Entry form template](ENTRYFORM.md)
  
# Hackathon Submission Entry form

## Team name
Will Work For Cache

## Category
Best use of AI

## Description
Our module uses AI to help content authors improve the quality of the metadata behind their site, rather than trying to generate net-new content. This allows content authors to focus on the creation and curation of high-quality content while enabling automation of the often-overlooked boilerplate tasks that can keep the site accessible and searchable.
* Image alt tag generation using Azure AI Vision Studio
* Keyword and meta text summarization using [TODO]

The module is also designed to be useful and usable across a wide variety of existing Sitecore projects, and to be easily extended for other use cases (e.g. additional text fields).

## Video link
⟹ Provide a video highlighing your Hackathon module submission and provide a link to the video. You can use any video hosting, file share or even upload the video to this repository. _Just remember to update the link below_ [TODO]

⟹ [Replace this Video link](#video-link)

## Pre-requisites and Dependencies

* SPE
* Azure resources created, with endpoint URLs and access keys generated for:
    * 1x Computer Vision instance - for image alt tag generation - free tier is fine
    * 1x Text Analytics instance - Azure Cognitive Service for Language - for text summarization

## Installation instructions

1. Use the Sitecore Installation wizard to install the [package - TODO](#link-to-package) - incl. core items, sample images, and the CM web.config
2. Update the configuration settings notes in the **Configuration** section below

> _A simple well-described installation process is required to win the Hackathon._  
> Feel free to use any of the following tools/formats as part of the installation:
> - Sitecore Package files
> - Docker image builds
> - Sitecore CLI
> - msbuild
> - npm / yarn
> 
> _Do not use_
> - TDS
> - Unicorn
 
for example:

1. Use the Sitecore Installation wizard to install the [package](#link-to-package)
2. ...
3. profit

### Configuration

#### Required Settings

Configure the following settings in */App_Config/Include/Feature/WillWorkForCache.Feature.GenerativeMetadata.config*:

* VISION_ENDPOINT and VISION_KEY should be the endpoint and access key for the Vision Services instance you want to use:
  
        <setting name="VISION_ENDPOINT" value="https://whfc2024-vision.cognitiveservices.azure.com/" />
        <setting name="VISION_KEY" value="0123456789abcdef0123456789abcdef" />

* LANGUAGE_ENDPOINT and LANGUAGE_KEY should be the endpoint and access key for the text analytics instance you want to use:

        <setting name="LANGUAGE_ENDPOINT" value="[https://whfc2024-vision.cognitiveservices.azure.com/](https://hackathonlanguageanalysis.cognitiveservices.azure.com/)" />
        <setting name="LANGUAGE_KEY" value="0123456789abcdef0123456789abcdef" />

* LOCALIZED_CM_URL should be set to the host name or IP address that can be used to retrieve content from the CM instance, i.e. by default, if running CM within a container, this will allow the CM server to download the contents of a rendered page from itself.

        <setting name="LOCALIZED_CM_URL" value="127.0.0.1" />

These are applied as Sitecore settings to take advantage of existing tooling - e.g. these values can easily be provided via environment variables in real containerized environments.

#### Optional Settings

The following settings can be updated to configure the minimum confidence and maximum number of tags that will be saved for media items.

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
 

### Future Improvements

* Experience Editor - Button for an edit frame for text fields?
* Experience Editor - Button for an edit frame for images in an image field?
* Powershell reporting (+ buttons from there?)
 
⟹ Provide documentation about your module, how do the users use your module, where are things located, what do the icons mean, are there any secret shortcuts etc.

Include screenshots where necessary. You can add images to the `./images` folder and then link to them from your documentation:

![Hackathon Logo](docs/images/hackathon.png?raw=true "Hackathon Logo")

You can embed images of different formats too:

![Deal With It](docs/images/deal-with-it.gif?raw=true "Deal With It")

And you can embed external images too:

![Random](https://thiscatdoesnotexist.com/)

## Comments
If you'd like to make additional comments that is important for your module entry.
