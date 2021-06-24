using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public class VideoAnalyzerClientConfiguration
    {
        /// <summary>
        /// Gets the authority endpoint base uri for Ava AAD app.
        /// </summary>
        public Uri AvaAadAuthorityEndpointBaseUri { get; set; }

        /// <summary>
        /// Gets the tenant id for Azure Video Analyzer.
        /// </summary>
        public string AvaTenantId { get; set; }

        /// <summary>
        /// Gets the AAD client id for Azure Video Analyzer.
        /// </summary>
        public string AvaClientAadClientId { get; set; }

        /// <summary>
        /// Gets the AAD certificate for Azure Video Analyzer.
        /// </summary>
        public string AvaClientAadSecret { get; set; }

        /// <summary>
        /// Gets the AAD resource for Azure Video Analyzer.
        /// </summary>
        public string AvaAadResource { get; set; }

        /// <summary>
        /// Gets the AVA media account name.
        /// </summary>
        public string AvaAccountName { get; set; }

        /// <summary>
        /// Gets the AVA media account resource group name.
        /// </summary>
        public string AvaAccountResourceGroupName { get; set; }

        /// <summary>
        /// Gets the AVA media account subscription id.
        /// </summary>
        public string AvaAccountSubscriptionId { get; set; }

        /// <summary>
        /// Gets the AVA ARM endpoint.
        /// </summary>
        public Uri AvaArmEndpoint { get; set; }
    }
}
