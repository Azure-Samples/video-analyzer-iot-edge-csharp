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
	public class LivePipelineManager
	{
        public PipelineTopology Build()
        {
            var properties = new PipelineTopologyProperties()
            {
                Description = "Analyzing live video to detect motion and emit events"
            };

            SetParameters(properties);
            SetSources(properties);
            SetProcessors(properties);
            SetSinks(properties);

            return new PipelineTopology("MotionDetection")
            {
                Properties = properties
            };

        }

        // Add parameters to Topology
        private void SetParameters(PipelineTopologyProperties properties)
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
        private void SetSources(PipelineTopologyProperties properties)
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
        private void SetProcessors(PipelineTopologyProperties properties)
        {
            properties.Processors.Add(
                new MotionDetectionProcessor("motionDetection", new List<NodeInput> { { new NodeInput("rtspSource") } })
                {
                    Sensitivity = "${motionSensitivity}"
                }
            );
        }

        // Add sinks to Topology
        private void SetSinks(PipelineTopologyProperties properties)
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
    }
}