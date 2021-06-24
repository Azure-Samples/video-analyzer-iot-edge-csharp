// -----------------------------------------------------------------------
//  <copyright company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.Media.LiveVideoAnalytics.Apis.Client.MediaGraphManager.RP20210501privatepreview.Models;
using System.Collections.Generic;

namespace SampleApp.Helpers
{
    /// <summary>
    /// Class to help generate LivePipelines.
    /// </summary>
    public class LivePipelineModelBuilder
    {
        /// <summary>
        /// Gets The LivePipeline.
        /// </summary>
        public LivePipeline LivePipeline { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LivePipelineModelBuilder"/> class.
        /// </summary>
        /// <param name="pipelineTopologyName">Name of the pipeline topology.</param>
        /// <param name="livePipelineName">Name of the LivePipeline.</param>
        /// <param name="graphInstanceDescription">Description of the LivePipeline.</param>
        public LivePipelineModelBuilder(string pipelineTopologyName, string livePipelineName, int bitrateKbps, string graphInstanceDescription = null)
        {
            Check.NotNull(pipelineTopologyName, nameof(pipelineTopologyName));
            Check.InRange(pipelineTopologyName.Length, 1, 32, nameof(pipelineTopologyName));
            Check.NotNull(livePipelineName, nameof(livePipelineName));

            LivePipeline = new LivePipeline(pipelineTopologyName, name: livePipelineName, description: graphInstanceDescription, bitrateKbps: bitrateKbps,
                parameters: new List<ParameterDefinition>());
        }

        /// <summary>
        /// Adds a Parameter to the LivePipeline's parameters collection.
        /// </summary>
        /// <param name="parameter">ParameterDefinition to add.</param>
        /// <returns>LivePipeline model builder.</returns>
        public LivePipelineModelBuilder AddParameter(ParameterDefinition parameter)
        {
            LivePipeline.Parameters.Add(parameter);
            return this;
        }

        /// <summary>
        /// Adds a list of Parameters to the LivePipeline's parameters collection.
        /// </summary>
        /// <param name="parameters">List<ParameterDefinition> to add.</param>
        /// <returns>LivePipeline model builder.</returns>
        public LivePipelineModelBuilder AddParameters(List<ParameterDefinition> parameters)
        {
            foreach (var parameter in parameters)
            {
                LivePipeline.Parameters.Add(parameter);
            }

            return this;
        }
    }

}
