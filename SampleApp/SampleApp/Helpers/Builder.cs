//-----------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Azure.Media.VideoAnalyzer.Edge.Models;
using System.Collections.Generic;

namespace SampleApp.Helpers
{
    /// <summary>
    /// A helper class to manage LivePipeline operations.
    /// </summary>
    public static class Builder
	{
        public static PipelineTopology BuildPipelineTopology(string pipelineTopologyName)
        {
            var properties = new PipelineTopologyProperties()
            {
                Description = "Sample pipeline topology"
            };

            SetParameters(properties);
            SetSources(properties);
            SetProcessors(properties);
            SetSinks(properties);

            return new PipelineTopology(pipelineTopologyName)
            {
                Properties = properties
            };

        }

        // Add parameters to Topology
        private static void SetParameters(PipelineTopologyProperties properties)
        {
            properties.Parameters.Add(
                new ParameterDeclaration("rtspUserName", ParameterType.String)
                {
                    Description = "rtsp source user name.",
                    Default = "dummyUserName"
                });

            properties.Parameters.Add(
                new ParameterDeclaration("rtspPassword", ParameterType.SecretString)
                {
                    Description = "rtsp source password.",
                    Default = "dummyPassword"
                });

            properties.Parameters.Add(
                new ParameterDeclaration("rtspUrl", ParameterType.String)
                {
                    Description = "rtsp Url."
                });

            properties.Parameters.Add(
               new ParameterDeclaration("inferencingUrl", ParameterType.String)
               {
                   Description = "inferencing url.",
                   Default = "http://yolov3/score"
               });

            properties.Parameters.Add(
               new ParameterDeclaration("inferencingUserName", ParameterType.String)
               {
                   Description = "inferencing UserName",
                   Default = "dummyusername"
               });

            properties.Parameters.Add(
               new ParameterDeclaration("inferencingPassword", ParameterType.String)
               {
                   Description = "inferencing Password",
                   Default = "dummypassword"
               });

            properties.Parameters.Add(
               new ParameterDeclaration("frameWidth", ParameterType.String)
               {
                   Description = "Width of the video frame to be received from AVA.",
                   Default = "416"
               });

            properties.Parameters.Add(
               new ParameterDeclaration("frameHeight", ParameterType.String)
               {
                   Description = "Height of the video frame to be received from AVA.",
                   Default = "416"
               });

            properties.Parameters.Add(
               new ParameterDeclaration("videoName", ParameterType.String)
               {
                   Description = "The name of the video"
               });
        }

        // Add sources to Topology
        private static void SetSources(PipelineTopologyProperties properties)
        {
            properties.Sources.Add(new RtspSource("rtspSource",
                new UnsecuredEndpoint("${rtspUrl}")
                {
                    Credentials = new UsernamePasswordCredentials("${rtspUserName}", "${rtspPassword}")
                }
                )
            );
        }

        // Add processors to Topology
        private static void SetProcessors(PipelineTopologyProperties properties)
        {
            properties.Processors.Add(
                new SignalGateProcessor("signalGateProcessor", new List<NodeInput> { { new NodeInput("rtspSource") } })
                {
                    ActivationEvaluationWindow = "PT1S",
                    ActivationSignalOffset = "-PT5S",
                    MinimumActivationTime = "PT30S",
                    MaximumActivationTime = "PT30S"
                }
            );

            properties.Processors.Add(new HttpExtension("inferenceClient", new List<NodeInput> { { new NodeInput("rtspSource") } },
               new UnsecuredEndpoint("${inferencingUrl}")
               {
                   Credentials = new UsernamePasswordCredentials("${inferencingUserName}", "${inferencingPassword}")
               },
               new ImageProperties()
               {
                   Format = new ImageFormatBmp(),
                   Scale = new ImageScale() { Height = "${frameHeight}", Width = "${frameWidth}", Mode = ImageScaleMode.PreserveAspectRatio }
               }
           ));
        }

        // Add sinks to Topology
        private static void SetSinks(PipelineTopologyProperties properties)
        {
            properties.Sinks.Add(
                new IotHubMessageSink(
                    "hubSink",
                    new List<NodeInput> {
                        { new NodeInput("inferenceClient") }
                    },
                    "inferenceOutput")
                );

            properties.Sinks.Add(
               new VideoSink("videoSink",
                   new List<NodeInput> {
                        { new NodeInput("signalGateProcessor") }
                   },
                   "${videoName}",
                   "/var/lib/azuremediaservices/tmp/",
                   "2048")
               {
                   VideoCreationProperties = new VideoCreationProperties()
                   {
                       Description = "EvrHubVideo sample video",
                       SegmentLength = "PT30S",
                       Title = "EvrHubVideo"
                   }
               }
           );
        }

        public static LivePipeline BuildLivePipepine(
            string livePipelineName,
            string pipelineTopologyName,
            string url,
            string userName,
            string password,
            string videoName)
        {
            var result = new LivePipeline(livePipelineName)
            {
                Properties = new LivePipelineProperties
                {
                    TopologyName = pipelineTopologyName,
                    Description = "Sample pipeline description"
                }
            };

            result.Properties.Parameters.Add(new ParameterDefinition("rtspUrl") { Value = url });
            result.Properties.Parameters.Add(new ParameterDefinition("rtspUserName") { Value = userName });
            result.Properties.Parameters.Add(new ParameterDefinition("rtspPassword") { Value = password });
            result.Properties.Parameters.Add(new ParameterDefinition("videoName") { Value = videoName});

            return result;
        }
    }
}