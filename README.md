---
page_type: sample
languages:
  - csharp
products:
  - azure
  - azure-video-analyzer
description: "The samples in this repo show how to use the Azure Video Analyzer module to analyze live video using AI modules of your choice and optionally record video on the edge device or in the cloud."  
---

# Azure Video Analyzer samples

This repository contains C# samples for Azure Video Analyzer

## Contents

| File/folder       | Description                                |
|----------------------|--------------------------------------------|
| `src`                | Sample source code.                        |
| `.gitignore`         | Defines what to ignore at commit time.     |
| `README.md`          | This README file.                          |
| `LICENSE`            | The license for the sample.                |
| `SECURITY`           | Guidelines for reporting security issues   |
| `CODE_OF_CONDUCT.md` | Open source code of conduct                |

The 'src' folder contains two sub-folders

* **cloud-to-device-console-app** - This folder contains a dotnet core console app that enables you to invoke direct methods of Azure Video Analyzer module, with parameters defined by you in a JSON file (operations.json).
* **edge** - This folder has a few IoT Edge deployment manifest templates, along with sample code for an IoT Edge module (under 'modules' folder) that can be used in conjunction with the Azure Video Analyzer module.
* **jwt-token-issuer** - This folder contains an application for generating [JSON Web Tokens](https://datatracker.ietf.org/doc/html/rfc7519). The output can then be used with the [Azure Video Analyzer player widgets](https://docs.microsoft.com/en-us/azure/azure-video-analyzer/video-analyzer-docs/player-widget) for playing back video recordings.

## Prerequisites

1. An active Azure subscription
2. Azure resources deployed in the Azure subscription

    a. Video Analyzer account

    b. Storage account

    c. Managed Identity

    d. IoT Hub
    
    e. Azure container registry

3. A Linux edge device with [IoT Edge runtime](https://docs.microsoft.com/azure/iot-edge/how-to-install-iot-edge-linux)

4. [Visual Studio Code](https://code.visualstudio.com/) on your development machine with following extensions

    a. [Azure IoT Tools](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-tools)

    b. [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)

5. [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1) on your development machine

6. [Docker](https://docs.docker.com/engine/install/) on your development machine

Set up Azure resources:

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://aka.ms/ava-click-to-deploy)

## Setup

After cloning the repository, follow instructions outlined in **src/cloud-to-device-console-app/readme.md** to setup the console app.

## Running the sample

Follow instructions outlined in **src/cloud-to-device-console-app/readme.md** to run the console app.

## Key concepts

Read [Azure Video Analyzer concepts](https://docs.microsoft.com/azure/azure-video-analyzer/video-analyzer-docs/overview)

## Code of conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
