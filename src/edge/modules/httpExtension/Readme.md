# Http Extension module

The HTTP extension module enables your own IoT Edge module to accept video frames as an http POST request. 

## Prerequisites

1. [Install Docker](http://docs.docker.com/docker-for-windows/install/) on your machine
2. Install [curl](http://curl.haxx.se/)

### Design
The HTTP extension module is an ASP.NET Core web api built to handle requests using controllers. Let's do a quick walkthrough the files:

*Program.cs*: this is the entry point of the application. It is responsible for the creation of the .NET Generic Host.

```
IHostBuilder CreateHostBuilder(string[] args)
```
This method creates and configures the Host, we specify in which url the HTTP extension module will listen on and specify the start up type.
In the excerpt below we set the the HTTP extension module to listen on port 8080. 

```
.UseUrls("http://*:8080")
.UseStartup<Startup>();
```
*Note:* Please keep in mind that if you modify the port you'll have to update it as well in the **EXPOSE** instruction of the Dockerfile.


*Startup.cs*: this class used for configuring services and the HTTP request pipeline.


*Controllers\ScoreController.cs*: this class is responsible for handling the http POST request sent from AVA. The request will contain in its body the decoded frame. The ScoreController reads the decoded frame from the request's body, builds an image and processes it using the ImageProcessor to get the Inference response. If the processing was successful a JSON represention of the Inference response is returned with http status code 200. In case there was an error, an instance of BadRequestObject containing the error description and http status 400 is returned.


*Processors\ImageProcessor.cs*: this class is responsible for processing the image. In a nutshell, it converts an image to grayscale and determines if its color intensity is dark or light. You can add your own processor logic by adding a new class and implementing the method:

```
InferenceResponse ProcessImage(Image image)
```

Once you've added the new class, you'll have to update the ScoreController so it instantiates your class and invokes the **ProcessImage** method on it to run your processing logic.

### Building, publishing and running the Docker container

To build the image, use the Docker file named `Dockerfile`.

First, a couple assumptions

* We'll be using Azure Container Registry (ACR) to publish our image before distributing it
* Our local Docker container image is already logged into ACR.
* Our hypothetical ACR name is "myregistry". Your name may defer, so please update it properly in the following commands.

> If you're unfamiliar with ACR or have any questions, please follow this [demo on building and pushing an image into ACR](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-docker-cli).

`cd` onto the http extension's root directory 

```
sudo docker build -t httpextension:latest .

sudo docker tag httpextension:latest myregistry.azurecr.io/httpextension:1

sudo docker push myregistry.azurecr.io/httpextension:1
```

Then, from the box where the container should execute, run this command:

`sudo docker run -d -p 8080:8080 --name httpextension myregistry.azurecr.io/httpextension:1`

Let's decompose it a bit:

* `-p 8080:8080`: it's up to you where you'd like to map the containers 8080 port. You can pick whatever port fits your needs.
* `--name`: the name of the running container.
* `registry/image:tag`: replace this with the corresponding location/image:tag where you've pushed the image built from the `Dockerfile`

### Updating references into Topologies, to target the HTTPS inferencing container address
The topology (i.e. https://github.com/Azure/video-analyzer/tree/main/pipelines/live/topologies/httpExtension/topology.json) must define an inferencing URL:

* Url Parameter
```
{
  "name": "inferencingUrl",
  "type": "String",
  "description": "inferencing Url",
  "default": "https://<REPLACE-WITH-IP-OR-CONTAINER-NAME>/score"
}
```
* Configuration
```
{
  "@apiVersion": "1.0",
  "name": "TopologyName",
  "properties": {
    "processors": [
      {
        "@type": "#Microsoft.VideoAnalyzer.RtspSource",
        "name": "inferenceClient",
        "endpoint": {
          "@type": "#Microsoft.VideoAnalyzer.UnsecuredEndpoint",
          "url": "${inferencingUrl}",
          "credentials": {
            "@type": "#Microsoft.VideoAnalyzer.UsernamePasswordCredentials",
            "username": "${inferencingUserName}",
            "password": "${inferencingPassword}"
          }
        },
        "image": {
          "scale":
          {
            "mode": "Pad",
            "width": "416",
            "height": "416"
          },
          "format":
          {
            "@type": "#Microsoft.VideoAnalyzer.ImageFormatJpeg",
            "quality": "90"
          }
        }
      }
    ]
  }
}
```
## Using the http extension container

Test the container using the following commands

### /score

To get the response of the processed image, use the following command

```bash
curl -X POST https://<REPLACE-WITH-IP-OR-CONTAINER-NAME>/score -H "Content-Type: image/jpeg" --data-binary @<image_file_in_jpeg>
```

If successful, you will see JSON printed on your screen that looks something like this

```JSON
{
  "inferences": [
    {
      "type": "classification",
      "subType": "colorIntensity",
      "classification": {
        "confidence": 1,
        "value": "dark"
      }
    }
  ]
}
```

Terminate the container using the following Docker commands

```bash
docker stop httpextension
docker rm httpextension
```

## Upload Docker image to Azure container registry

Follow instructions in [Push and Pull Docker images  - Azure Container Registry](http://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-docker-cli) to save your image for later use on another machine.

## Deploy as an Azure IoT Edge module

Follow instruction in [Deploy module from Azure portal](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-deploy-modules-portal) to deploy the container image as an IoT Edge module (use the IoT Edge module option).
