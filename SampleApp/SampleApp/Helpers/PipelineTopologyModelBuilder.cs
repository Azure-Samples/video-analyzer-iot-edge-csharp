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
    /// Class to help generate pipeline topologies.
    /// </summary>
    public class PipelineTopologyModelBuilder
    {
        /// <summary>
        /// Gets The pipeline topology.
        /// </summary>
        public PipelineTopology PipelineTopology { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineTopologyModelBuilder"/> class.
        /// </summary>
        /// <param name="pipelineTopologyName">Name of the pipeline topology.</param>
        /// <param name="pipelineTopologyDescription">Description of the pipeline topology.</param>
        public PipelineTopologyModelBuilder(string pipelineTopologyName, string pipelineTopologyDescription = null)
        {
            Check.NotNull(pipelineTopologyName, nameof(pipelineTopologyName));
            Check.InRange(pipelineTopologyName.Length, 1, 32, nameof(pipelineTopologyName));

            PipelineTopology = new PipelineTopology(new List<SourceNodeBase>(),
                new List<SinkNodeBase>(), name: pipelineTopologyName, parameters: new List<ParameterDeclaration>(),
                processors: new List<ProcessorNodeBase>(), description: pipelineTopologyDescription);
        }

        /// <summary>
        /// Adds a Source to the topology's sources collection.
        /// </summary>
        /// <param name="source">SourceNodeBase to add.</param>
        /// <returns>PipelineTopology model builder.</returns>
        public PipelineTopologyModelBuilder AddSource(SourceNodeBase source)
        {
            PipelineTopology.Sources.Add(source);
            return this;
        }

        /// <summary>
        /// Adds a Processor to the pipeline topology's processors collection.
        /// </summary>
        /// <param name="processor">ProcessorNodeBase to add.</param>
        /// <returns> PipelineTopology model builder.</returns>
        public PipelineTopologyModelBuilder AddProcessor(ProcessorNodeBase processor)
        {
            PipelineTopology.Processors.Add(processor);
            return this;
        }

        /// <summary>
        /// Adds a Sink to the pipeline topology's sink collection.
        /// </summary>
        /// <param name="sink">SinkNodeBase to add.</param>
        /// <returns> PipelineTopology model builder.</returns>
        public PipelineTopologyModelBuilder AddSink(SinkNodeBase sink)
        {
            PipelineTopology.Sinks.Add(sink);
            return this;
        }

        /// <summary>
        /// Adds a list of Sinks to the topology's sink collection.
        /// </summary>
        /// <param name="sink">SinkNodeBase to add.</param>
        /// <returns> PipelineTopology model builder.</returns>
        public PipelineTopologyModelBuilder AddSinks(List<SinkNodeBase> sinks)
        {
            foreach (var sink in sinks)
            {
                PipelineTopology.Sinks.Add(sink);
            }

            return this;
        }

        /// <summary>
        /// Adds a Parameter to the pipeline topology's parameters collection.
        /// </summary>
        /// <param name="parameter">ParameterDeclaration to add.</param>
        /// <returns>PipelineTopology model builder.</returns>
        public PipelineTopologyModelBuilder AddParameter(ParameterDeclaration parameter)
        {
            PipelineTopology.Parameters.Add(parameter);
            return this;
        }

        /// <summary>
        /// Adds a list of Parameters to the pipeline topology's parameters collection.
        /// </summary>
        /// <param name="parameters">List<ParameterDeclaration> to add.</param>
        /// <returns>PipelineTopology model builder.</returns>
        public PipelineTopologyModelBuilder AddParameters(List<ParameterDeclaration> parameters)
        {
            foreach (var parameter in parameters)
            {
                PipelineTopology.Parameters.Add(parameter);
            }

            return this;
        }
    }
}
