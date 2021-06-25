// -----------------------------------------------------------------------
//  <copyright company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace SampleApp.Models
{
    public class VideoAnalyzerClientConfiguration
    {

        /// <summary>
        /// The IotHubConnectionString.
        /// </summary>
        public string IotHubConnectionString { get; set; }

        /// <summary>
        /// The device id.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// The module id
        /// </summary>
        public string ModuleId { get; set; }
    }
}
