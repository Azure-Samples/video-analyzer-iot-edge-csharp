//-----------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Media.LiveVideoAnalytics.Apis.Client.MediaGraphManager.RP20210501privatepreview.Models;
using System.Collections.Generic;

namespace SampleApp.Helpers
{
	/// <summary>
	/// A helper class to manage LivePipeline operations.
	/// </summary>
	public static class LivePipelineManager
	{
		private const string LivePipelineDescription = "LivePipeline Description";
		private const string PipelineTopologyDescription = "Pipeline Topology Description";
		private const string RtspSource = "RtspSource";

		private const string VideoNameParameterName = "videoNameParameter";
		private const string RtspUrlParameterName = "rtspUrlParameter";
		private const string RtspPasswordParameterName = "rtspPasswordParameter";
		private const string RtspUsernameParameterName = "rtspUsernameParameter";
		private const string RtspIoTHubArmIdName = "rtspIoTHubArmIdParameter";
		private const string RtspDeviceIdName = "rtspDeviceIdParameter";
		private const string RtspUserAssignedManagedIdentityArmIdName = "RtspUserAssignedManagedIdentityArmIdParameter";

		/// <summary>
		/// Create PipelineTopology model.
		/// </summary>
		/// <param name="pipelineTopologyName">PipelineTopology name.</param>
		/// <returns>PipelineTopology model.</returns>
		public static PipelineTopology CreatePipelineTopologyModel(string pipelineTopologyName)
		{
			return new PipelineTopologyModelBuilder(pipelineTopologyName, PipelineTopologyDescription)
			   .AddSource(
					new RtspSource
					{
						Name = RtspSource,
						Transport = "tcp",
						Endpoint = new UnsecuredEndpoint
						{
							Url = "${" + RtspUrlParameterName + "}",
							Credentials = new UsernamePasswordCredentials
							{
								Username = "${" + RtspUsernameParameterName + "}",
								Password = "${" + RtspPasswordParameterName + "}",
							},
							Tunnel = new IotSecureDeviceRemoteTunnel("${" + RtspIoTHubArmIdName + "}", "${" + RtspDeviceIdName + "}", "${" + RtspUserAssignedManagedIdentityArmIdName + "}")
						},
					})
			   .AddSink(
					new VideoSink
					{
						Name = "VideoSink",
						VideoName = "${" + VideoNameParameterName + "}",
						Inputs = new List<NodeInput>
						{
							new NodeInput
							{
								NodeName = RtspSource,
							},
						},
						VideoCreationProperties = new VideoCreationProperties
						{
							Title = "Sample Video",
							Description = "Sample video description",
							SegmentLength = "PT30S",
						},
					})
			   .AddParameters(
					new List<ParameterDeclaration>
					{
						new ParameterDeclaration
						{
							Name = VideoNameParameterName,
							Type = ParameterDeclarationType.String,
							DefaultProperty = "defaultVideo",
							Description = "video name parameter",
						},
						new ParameterDeclaration
						{
							Name = RtspUsernameParameterName,
							Type = ParameterDeclarationType.String,
							DefaultProperty = "defaultUsername",
							Description = "rtsp username parameter",
						},
						new ParameterDeclaration
						{
							Name = RtspPasswordParameterName,
							Type = ParameterDeclarationType.SecretString,
							DefaultProperty = "defaultPassword",
							Description = "rtsp password parameter",
						},
						new ParameterDeclaration
						{
							Name = RtspUrlParameterName,
							Type = ParameterDeclarationType.String,
							DefaultProperty = "rtsp://microsoft.com/defaultUrl",
							Description = "rtsp url parameter",
						},
						new ParameterDeclaration
						{
							Name = RtspIoTHubArmIdName,
							Type = ParameterDeclarationType.String,
							Description = "rtsp iot hub arm id",
						},
						new ParameterDeclaration
						{
							Name = RtspDeviceIdName,
							Type = ParameterDeclarationType.String,
							Description = "rtsp device id",
						},
						new ParameterDeclaration
						{
							Name = RtspUserAssignedManagedIdentityArmIdName,
							Type = ParameterDeclarationType.String,
							Description = "rtsp user assigned managed identity arm id",
						}
					})
			   .PipelineTopology;
		}

		/// <summary>
		/// Create PipelineTopology model.
		/// </summary>
		/// <param name="pipelineTopologyName">PipelineTopology name.</param>
		/// <returns>PipelineTopology model.</returns>
		public static PipelineTopology CreatePlaybackPipelineTopologyModel(string pipelineTopologyName, string audience, string issuer, string modulus, string exponent, string claimName, string claimValue)
		{
			return new PipelineTopologyModelBuilder(pipelineTopologyName, PipelineTopologyDescription)
			   .AddSource(
					new RtspSource
					{
						Name = RtspSource,
						Transport = "tcp",
						Endpoint = new UnsecuredEndpoint
						{
							Url = "${" + RtspUrlParameterName + "}",
							Credentials = new UsernamePasswordCredentials
							{
								Username = "${" + RtspUsernameParameterName + "}",
								Password = "${" + RtspPasswordParameterName + "}",
							},
							Tunnel = new IotSecureDeviceRemoteTunnel("${" + RtspIoTHubArmIdName + "}", "${" + RtspDeviceIdName + "}", "${" + RtspUserAssignedManagedIdentityArmIdName + "}")
						},
					})
			   .AddSinks(
					new List<SinkNodeBase> {
						new VideoSink
						{
							Name = "VideoSink",
							VideoName = "",
							Inputs = new List<NodeInput>
							{
								new NodeInput
								{
									NodeName = RtspSource,
								},
							},
						VideoCreationProperties = new VideoCreationProperties
						{
							Title = "Sample Video",
							Description = "Sample video description",
							SegmentLength = "PT30S",
						}
						}
					})
			   .AddParameters(
					new List<ParameterDeclaration>
					{
						new ParameterDeclaration
						{
							Name = VideoNameParameterName,
							Type = ParameterDeclarationType.String,
							DefaultProperty = "defaultVideo",
							Description = "video name parameter",
						},
						new ParameterDeclaration
						{
							Name = RtspUsernameParameterName,
							Type = ParameterDeclarationType.String,
							DefaultProperty = "defaultUsername",
							Description = "rtsp username parameter",
						},
						new ParameterDeclaration
						{
							Name = RtspPasswordParameterName,
							Type = ParameterDeclarationType.SecretString,
							DefaultProperty = "defaultPassword",
							Description = "rtsp password parameter",
						},
						new ParameterDeclaration
						{
							Name = RtspUrlParameterName,
							Type = ParameterDeclarationType.String,
							DefaultProperty = "rtsp://microsoft.com/defaultUrl",
							Description = "rtsp url parameter",
						},
						new ParameterDeclaration
						{
							Name = RtspIoTHubArmIdName,
							Type = ParameterDeclarationType.String,
							Description = "rtsp iot hub arm id",
						},
						new ParameterDeclaration
						{
							Name = RtspDeviceIdName,
							Type = ParameterDeclarationType.String,
							Description = "rtsp device id",
						},
						new ParameterDeclaration
						{
							Name = RtspUserAssignedManagedIdentityArmIdName,
							Type = ParameterDeclarationType.String,
							Description = "rtsp user assigned managed identity arm id",
						}
					})
			   .PipelineTopology;
		}

		/// <summary>
		/// Create LivePipeline model.
		/// </summary>
		/// <param name="settings">Media graph test settings.</param>
		/// <param name="graphInstanceName">LivePipeline name.</param>
		/// <param name="pipelineTopologyName">PipelineTopology name.</param>
		/// <param name="bitrateKbps">The bit rate in Kbps.</param>
		/// <param name="videoName">Video name.</param>
		/// <param name="rtspSourceUrl">Rtsp source URL.</param>
		/// <param name="rtspIotHubArmId">Rtsp source IoT Hub Arm ID.</param>
		/// <param name="rtspDeviceId">Rtsp device ID.</param>
		/// <param name="rtspUsername">Rtsp username.</param>
		/// <param name="rtspPassword">Rtsp password.</param>
		/// <returns>LivePipeline model.</returns>
		public static LivePipeline CreateLivePipelineModel(
			string graphInstanceName,
			string pipelineTopologyName,
			int bitrateKbps,
			string videoName,
			string rtspSourceUrl,
			string rtspIotHubArmId,
			string rtspDeviceId,
			string rtspUsername,
			string rtspPassword)
		{
			return new LivePipelineModelBuilder(pipelineTopologyName, graphInstanceName, bitrateKbps, LivePipelineDescription)
				.AddParameters(
					new List<ParameterDefinition>
					{
						new ParameterDefinition
						{
							Name = VideoNameParameterName,
							Value = videoName,
						},
						new ParameterDefinition
						{
							Name = RtspUsernameParameterName,
							Value = rtspUsername,
						},
						new ParameterDefinition
						{
							Name = RtspPasswordParameterName,
							Value = rtspPassword,
						},
						new ParameterDefinition
						{
							Name = RtspUrlParameterName,
							Value = rtspSourceUrl,
						},
						new ParameterDefinition
						{
							Name = RtspIoTHubArmIdName,
							Value = rtspIotHubArmId,
						},
						new ParameterDefinition
						{
							Name = RtspDeviceIdName,
							Value = rtspDeviceId,
						},
					})
				.LivePipeline;
		}
	}
}