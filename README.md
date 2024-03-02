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
* Azure resources created, with endpoint URLs and access keys generated:
    * 1x Computer Vision - for image alt tag generation - free tier is fine
    * 1x XXX - for text summarization - xxxx TODO

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

Update the following keys in *Feature.Hackathon.config* [TODO change name]:

* VISION_ENDPOINT and VISION_KEY should be the endpoint and access key for the Vision Services instance you want to use:
  
        <setting name="VISION_ENDPOINT" value="https://whfc2024-vision.cognitiveservices.azure.com/" />
        <setting name="VISION_KEY" value="0123456789abcdef0123456789abcdef" />

* XXX and YYY [TODO]

These are applied as Sitecore settings to take advantage of existing tooling - e.g. these values can easily be provided via environment variables in real containerized environments.

## Usage instructions

* Content Editor > Image items or Media Library > Image items 
    * Use the "Generate Alt Text" ribbon button on the Media tab - the generated alt text will be inserted into the 'Alt' field for review
* Content Editor > Page items
    * Use the "Summarize" ribbon button on the [TODO] tab - the generated summary text will be inserted into the [TODO] field(s) for review

Future plans:
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
