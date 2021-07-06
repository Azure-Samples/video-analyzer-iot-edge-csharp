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
               new ParameterDeclaration("motionSensitivity", ParameterType.String)
               {
                   Description = "motion detection sensitivity.",
                   Default = "medium"
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
                new MotionDetectionProcessor("motionDetection", new List<NodeInput> { { new NodeInput("rtspSource") } })
                {
                    Sensitivity = "${motionSensitivity}"
                }
            );
        }

        // Add sinks to Topology
        private static void SetSinks(PipelineTopologyProperties properties)
        {
            properties.Sinks.Add(
                new IotHubMessageSink(
                    "hubSink",
                    new List<NodeInput> {
                        { new NodeInput("motionDetection") }
                    },
                    "inferenceOutput")
                );
        }

        public static LivePipeline BuildLivePipepine(
            string livePipelineName,
            string pipelineTopologyName,
            string url,
            string userName,
            string password)
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
           
            return result;
        }
    }
}