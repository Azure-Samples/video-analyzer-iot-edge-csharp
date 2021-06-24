//-----------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Media.LiveVideoAnalytics.Apis.Client.MediaGraphManager.RP20210501privatepreview;
using Microsoft.Media.LiveVideoAnalytics.Apis.Client.Videos.RP20210501privatepreview;
using Microsoft.Rest.Azure.Authentication;
using SampleApp.Models;

namespace SampleApp.Helpers
{
    /// <summary>
    /// A factory class to create an implementation of <see cref="IVideoAnalyzerClient"/>.
    /// </summary>
    public static class VideoAnalyzerClientFactory
    {
        /// <summary>
        /// By default, <see cref="HttpClient"/> times out after 100 seconds. Responses should not take this long, and recent
        /// ARM related errors have been determined within a few seconds even though the runner did not receive a response.
        /// By lowering the timeout the request can be retried more times within the same amount of total time.
        /// </summary>
        internal static readonly TimeSpan HttpClientTimeout = TimeSpan.FromSeconds(40);

        /// <summary>
        /// Creates an implementation of <see cref="IVideoAnalyzerClient"/> based on configuration.
        /// </summary>
        /// <param name="configuration">The <see cref="IVideoAnalyzerClientConfiguration"/>.</param>
        /// <param name="handler">The <see cref="DelegatingHandler"/>.</param>
        /// <returns>The created instance of <see cref="IVideoAnalyzerClient"/>.</returns>
        public static async Task<VideoAnalyzerClient> CreateAsync(VideoAnalyzerClientConfiguration configuration, DelegatingHandler? handler = null)
        {
            Check.NotNull(configuration, nameof(configuration));

            var aadSettings = new ActiveDirectoryServiceSettings
            {
                AuthenticationEndpoint = configuration.AvaAadAuthorityEndpointBaseUri,
                TokenAudience = new Uri(configuration.AvaAadResource),
                ValidateAuthority = true,
            };

            var clientCredentials = await ApplicationTokenProvider.LoginSilentAsync(
                configuration.AvaTenantId,
                configuration.AvaClientAadClientId,
                configuration.AvaClientAadSecret,
                aadSettings);

            // The delegating handler is not disposed by default. So we pass the handler by caller and dispose it outside.
            var videosClient = new VideoApplicationResourceProviderClient(configuration.AvaArmEndpoint, clientCredentials)
            {
                SubscriptionId = configuration.AvaAccountSubscriptionId,
                HttpClient = { Timeout = HttpClientTimeout },
            };

            var mgmClient = new MediaGraphManagerResourceProviderClient(configuration.AvaArmEndpoint, clientCredentials)
            {
                SubscriptionId = configuration.AvaAccountSubscriptionId,
                HttpClient = { Timeout = HttpClientTimeout },
            };

            return new VideoAnalyzerClient(
                videosClient,
                mgmClient,
                configuration.AvaAccountResourceGroupName,
                configuration.AvaAccountName);
        }
    }
}
