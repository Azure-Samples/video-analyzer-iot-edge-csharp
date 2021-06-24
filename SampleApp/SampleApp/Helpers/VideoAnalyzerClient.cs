//-----------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Media.LiveVideoAnalytics.Apis.Client.MediaGraphManager.RP20210501privatepreview;
using Microsoft.Media.LiveVideoAnalytics.Apis.Client.MediaGraphManager.RP20210501privatepreview.Models;
using Microsoft.Media.LiveVideoAnalytics.Apis.Client.Videos.RP20210501privatepreview;
using Microsoft.Media.LiveVideoAnalytics.Apis.Client.Videos.RP20210501privatepreview.Models;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ErrorResponseException = Microsoft.Media.LiveVideoAnalytics.Apis.Client.Videos.RP20210501privatepreview.Models.ErrorResponseException;
using MgmErrorResponseException = Microsoft.Media.LiveVideoAnalytics.Apis.Client.MediaGraphManager.RP20210501privatepreview.Models.ErrorResponseException;

namespace SampleApp.Helpers
{
    public class VideoAnalyzerClient 
    {
        private const int MaxRetries = 3;
        private const string PreservedVideoDescription = "Preserved";

        private readonly string _accountResourceGroupName;
        private readonly string _accountName;

        private readonly IVideoApplicationResourceProviderClient _videosClient;
        private readonly ConcurrentBag<string> _preservedVideoNames = new ConcurrentBag<string>();
        private readonly ConcurrentBag<string> _trackedVideoNamesForCleanup = new ConcurrentBag<string>();
        private readonly ConcurrentBag<string> _trackedAccessPolicyNamesForCleanup = new ConcurrentBag<string>();

        private readonly IMediaGraphManagerResourceProviderClient _mediaGraphManagerClient;
        private readonly ConcurrentBag<string> _trackedPipelineTopologiesForCleanup = new ConcurrentBag<string>();
        private readonly ConcurrentBag<string> _trackedLivePipelineForCleanup = new ConcurrentBag<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoAnalyzerClient"/> class.
        /// </summary>
        /// <param name="videosClient"> The <see cref="VideoApplicationResourceProviderClient"/>.</param>
        /// <param name="mediaGraphManagerClient">The <see cref="MediaGraphManagerResourceProviderClient"/>.</param>
        /// <param name="resourceGroupName">The AMS account resource group name.</param>
        /// <param name="accountName">The AMS account name.</param>
        public VideoAnalyzerClient(
            IVideoApplicationResourceProviderClient videosClient,
            IMediaGraphManagerResourceProviderClient mediaGraphManagerClient,
            string resourceGroupName,
            string accountName)
        {
            Check.NotNull(videosClient, nameof(videosClient));
            Check.NotNull(mediaGraphManagerClient, nameof(mediaGraphManagerClient));
            Check.NotNullOrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Check.NotNullOrWhiteSpace(accountName, nameof(accountName));

            _accountResourceGroupName = resourceGroupName;
            _accountName = accountName;

            _videosClient = videosClient;
            _mediaGraphManagerClient = mediaGraphManagerClient;
        }

        /// <inheritdoc />
        public async Task<VideoEntity> CreateOrUpdateVideoAsync(VideoEntity video, bool trackForCleanup = true, CancellationToken cancellationToken = default)
        {
            Check.NotNull(video, nameof(video));

            if (trackForCleanup)
            {
                _trackedVideoNamesForCleanup.Add(video.Name);
            }

            return await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
                async () => await _videosClient.Videos.CreateOrUpdateAsync(
                    _accountResourceGroupName,
                    _accountName,
                    video.Name,
                    video,
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task<VideoEntity> GetVideoAsync(string videoName, CancellationToken cancellationToken = default)
        {
            return await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
                async () => await _videosClient.Videos.GetAsync(
                    _accountResourceGroupName,
                    _accountName,
                    videoName,
                    cancellationToken));
        }

        /// <inheritdoc/>
        public async Task<List<VideoEntity>> GetAllVideosAsync(CancellationToken cancellationToken = default)
        {
            return await ListAllEntitiesAsync(
                async () => await _videosClient.Videos.ListAsync(_accountResourceGroupName, _accountName, cancellationToken: cancellationToken),
                async token => await _videosClient.Videos.ListNextAsync(token, cancellationToken),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        //public async Task<string> CreateOrUpdateAccessPolicyAndTokenAsync(string accessPolicyName, string brupTokenAudience, bool trackForCleanup = true, CancellationToken cancellationToken = default)
        //{
        //    Check.NotNullOrWhiteSpace(accessPolicyName, nameof(accessPolicyName));

        //    if (trackForCleanup)
        //    {
        //        _trackedAccessPolicyNamesForCleanup.Add(accessPolicyName);
        //    }

        //    var cryptoProvider = new InMemoryRsaCryptoProvider();

        //    var rsaParameters = cryptoProvider.GetRsaParameters();
        //    var exponent = Convert.ToBase64String(rsaParameters.Exponent);
        //    var modulus = Convert.ToBase64String(rsaParameters.Modulus);

        //    var issuers = new List<string> { "TestIssuer" };
        //    var brupAudiences = new List<string> { "${System.Runtime.BaseResourceUrlPattern}" };

        //    var accessPolicyModel = new AccessPolicyEntity(
        //        authentication: new JwtAuthentication(
        //            issuers,
        //            brupAudiences,
        //            keys: new List<TokenKey> { new RsaTokenKey(cryptoProvider.SigningKeyCredentials.Kid, "RS256", modulus, exponent) }),
        //        name: accessPolicyName);

        //    var accessPolicy = await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
        //        async () => await _videosClient.AccessPolicies.CreateOrUpdateAsync(
        //            _accountResourceGroupName,
        //            _accountName,
        //            accessPolicyModel.Name,
        //            accessPolicyModel,
        //            cancellationToken));

        //    if (accessPolicy == null)
        //    {
        //        var errorMessage = $"Access policy - {accessPolicyName} was not able to be created or updated.";

        //        Logger.LogWarning(
        //            LogCategory.TestCommon,
        //            EventId.TestCommonVideoAnalyzerArmClientFailure,
        //            errorMessage);

        //        throw new Exception(errorMessage);
        //    }

        //    var claims = new JwtClaims
        //    {
        //        Audiences = new List<string> { brupTokenAudience },
        //        Issuer = issuers[0],
        //        ExpirationDate = DateTime.UtcNow.AddDays(1),
        //    };

        //    var token = new JwtProvider(cryptoProvider).CreateToken(claims);
        //    return token;
        //}

        /// <inheritdoc />
        public async Task<AccessPolicyEntity> GetAccessPolicyAsync(string accessPolicyName, CancellationToken cancellationToken = default)
        {
            return await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
                async () => await _videosClient.AccessPolicies.GetAsync(
                    _accountResourceGroupName,
                    _accountName,
                    accessPolicyName,
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task<List<AccessPolicyEntity>> GetAllAccessPoliciesAsync(CancellationToken cancellationToken = default)
        {
            return await ListAllEntitiesAsync(
                async () => await _videosClient.AccessPolicies.ListAsync(_accountResourceGroupName, _accountName, cancellationToken: cancellationToken),
                async token => await _videosClient.AccessPolicies.ListNextAsync(token, cancellationToken),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteVideoAsync(string videoName, CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(videoName, nameof(videoName));
            var retryPolicy = GetTransientErrorRetryPolicy(cancellationToken);

            try
            {
                VideoEntity? video = null;
                try
                {
                    video = await retryPolicy.ExecuteAsync(
                        async () => await _videosClient.Videos.GetAsync(
                            _accountResourceGroupName,
                            _accountName,
                            videoName,
                            cancellationToken));
                }
                catch (ErrorResponseException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine(
                        "Video {0} not found. Skipping deletion.",
                        videoName);

                    return;
                }

                if (video.Description == PreservedVideoDescription)
                {
                    Console.WriteLine(
                        $"Skip clean up for video {videoName} because it is preserved.");

                    return;
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(
                    $"{ex}: Failed to get video {videoName}");

                throw;
            }

            await retryPolicy.ExecuteAsync(
                async () => await _videosClient.Videos.DeleteAsync(
                    _accountResourceGroupName,
                    _accountName,
                    videoName,
                    cancellationToken));
        }

        /// <inheritdoc/>
        public async Task<string> GetStreamingTokenAsync(string videoName, CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(videoName, nameof(videoName));
            var retryPolicy = GetTransientErrorRetryPolicy(cancellationToken);
            var result = await retryPolicy.ExecuteAsync(
                async () => await _videosClient.Videos.ListStreamingTokenAsync(
                    _accountResourceGroupName,
                    _accountName,
                    videoName,
                    cancellationToken));
            return result.Token;
        }

        /// <inheritdoc />
        public async Task DeletePipelineTopologyAsync(string name, CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            var retryPolicy = GetTransientErrorRetryPolicy(cancellationToken);

            try
            {
                var pipelineTopology = await retryPolicy.ExecuteAsync(
                    async () => await _mediaGraphManagerClient.PipelineTopologies.GetAsync(
                        _accountResourceGroupName,
                        _accountName,
                        name,
                        cancellationToken));

                if (pipelineTopology == null)
                {
                    Console.WriteLine($"Pipeline topology {name} not found. Skipping deletion.");
                    return;
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"{ex}: Failed to get pipeline topology {name}");

                throw;
            }

            await retryPolicy.ExecuteAsync(
                async () => await _mediaGraphManagerClient.PipelineTopologies.DeleteAsync(
                    _accountResourceGroupName,
                    _accountName,
                    name,
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task DeleteAccessPolicyAsync(string name, CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            var retryPolicy = GetTransientErrorRetryPolicy(cancellationToken);

            try
            {
                AccessPolicyEntity? accessPolicy = null;
                try
                {
                    accessPolicy = await retryPolicy.ExecuteAsync(
                        async () => await _videosClient.AccessPolicies.GetAsync(
                            _accountResourceGroupName,
                            _accountName,
                            name,
                            cancellationToken));
                }
                catch (ErrorResponseException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine(
                         "Access policy {0} not found. Skipping deletion.",
                        name);

                    return;
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"{ex}: Failed to get access policy {name}");

                throw;
            }

            await retryPolicy.ExecuteAsync(
                async () => await _videosClient.AccessPolicies.DeleteAsync(
                    _accountResourceGroupName,
                    _accountName,
                    name,
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task<PipelineTopology> CreatePipelineTopologyAsync(string name, PipelineTopology pipelineTopology, bool trackForCleanup = true, CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            if (trackForCleanup)
            {
                _trackedPipelineTopologiesForCleanup.Add(name);
            }

            return await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
                async () => await _mediaGraphManagerClient.PipelineTopologies.CreateOrUpdateAsync(
                    _accountResourceGroupName,
                    _accountName,
                    name,
                    pipelineTopology,
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task<PipelineTopology?> GetPipelineTopologyAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
                    async () => await _mediaGraphManagerClient.PipelineTopologies.GetAsync(
                        _accountResourceGroupName,
                        _accountName,
                        name,
                        cancellationToken));
            }
            catch (MgmErrorResponseException exception) when (exception.Response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine(
                     $"Failed to get pipeline topology {name}");

                return null;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(
                    $"{ex}: Failed to get pipeline topology {name}");

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<PipelineTopology>> GetAllPipelineTopologyAsync(CancellationToken cancellationToken = default)
        {
            return await ListAllEntitiesAsync(
                async () => await _mediaGraphManagerClient.PipelineTopologies.ListAsync(_accountResourceGroupName, _accountName, cancellationToken: cancellationToken),
                async token => await _mediaGraphManagerClient.PipelineTopologies.ListNextAsync(token, cancellationToken),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<LivePipeline> CreateLivePipelineAsync(string name, LivePipeline livePipeline, bool trackForCleanup = true, CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            if (trackForCleanup)
            {
                _trackedLivePipelineForCleanup.Add(name);
            }

            return await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
                async () => await _mediaGraphManagerClient.LivePipelines.CreateOrUpdateAsync(
                    _accountResourceGroupName,
                    _accountName,
                    name,
                    livePipeline,
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task<LivePipeline?> GetLivePipelineAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
                    async () => await _mediaGraphManagerClient.LivePipelines.GetAsync(
                        _accountResourceGroupName,
                        _accountName,
                        name,
                        cancellationToken));
            }
            catch (MgmErrorResponseException exception) when (exception.Response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.Write(
                    $"Failed to get live pipeline {name}");

                return null;
            }
            catch (Exception ex) 
            {
                Console.Write(
                    $"{ex}: Failed to get live pipeline {name}");

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<LivePipeline>> GetAllLivePipelineAsync(CancellationToken cancellationToken = default)
        {
            return await ListAllEntitiesAsync(
                async () => await _mediaGraphManagerClient.LivePipelines.ListAsync(_accountResourceGroupName, _accountName, cancellationToken: cancellationToken),
                async token => await _mediaGraphManagerClient.LivePipelines.ListNextAsync(token, cancellationToken),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteLivePipelineAsync(string name, CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            var retryPolicy = GetTransientErrorRetryPolicy(cancellationToken);

            try
            {
                var livePipeline = await GetLivePipelineAsync(name, cancellationToken);

                if (livePipeline == null)
                {
                    Console.Write(
                        "Live Pipeline {0} not found. Skipping deletion.",
                        name);

                    return;
                }
            }
            catch (Exception ex) 
            {
                Console.Write(
                     $"{ex}: Failed to get live pipeline {name}");

                throw;
            }

            await retryPolicy.ExecuteAsync(
                async () => await _mediaGraphManagerClient.LivePipelines.DeleteAsync(
                    _accountResourceGroupName,
                    _accountName,
                    name,
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task ActivateLivePipelineAsync(string name, CancellationToken cancellationToken = default)
        {
            await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
                async () => await _mediaGraphManagerClient.LivePipelines.ActivateAsync(
                    _accountResourceGroupName,
                    _accountName,
                    name,
                    cancellationToken));
        }

        /// <inheritdoc />
        public async Task DeactivateLivePipelineAsync(string name, CancellationToken cancellationToken = default)
        {
            await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
                async () => await _mediaGraphManagerClient.LivePipelines.DeactivateAsync(
                    _accountResourceGroupName,
                    _accountName,
                    name,
                    cancellationToken));
        }

        ///// <inheritdoc />
        //public async Task<VideoAnalyzerModel> GetVideoAnalyzerAccountAsync(CancellationToken cancellationToken = default)
        //{
        //    return await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
        //        async () => await _accountServiceClient.VideoAnalyzers.GetAsync(
        //            _accountName,
        //            cancellationToken));
        //}

        ///// <inheritdoc />
        //public async Task ValidateManifestAsync(
        //    string videoName,
        //    bool shouldBeStreaming,
        //    TimeSpan manifestCheckWaitDuration,
        //    bool logManifest = true,
        //    CancellationToken cancellationToken = default)
        //{
        //    Check.NotNullOrWhiteSpace(videoName, nameof(videoName));

        //    var (streamingUrl, playbackToken) = await GetVideoArchiveStreamingUrlAndPlaybackTokenAsync(videoName, cancellationToken);

        //    try
        //    {
        //        await StreamingManifestValidator.ValidateManifestAsync(streamingUrl, playbackToken, shouldBeStreaming, manifestCheckWaitDuration, cancellationToken);
        //    }
        //    catch (ManifestInvalidException exception)
        //    {
        //        PreserveMediaDiagnostics(videoName, logManifest, exception);

        //        throw;
        //    }
        //}

        ///// <inheritdoc/>
        //public async Task<Uri?> ValidatePlaybackAsync(string videoName, string livePipelineName, bool shouldPlayback, Uri? playbackUrl = null, CancellationToken cancellationToken = default)
        //{
        //    using var playbackDeviceTestClient = new PlaybackDeviceTestClient();

        //    var retryPolicy = new RetryPolicyBuilder { NumberOfRetries = 30 }
        //        .RetryOnResult<VideoEntity>((videoEntity) => videoEntity?.Streaming?.RtspTunnelUrl == null)
        //        .Build(TimeSpan.FromSeconds(3));

        //    var video = await retryPolicy.ExecuteAsync(
        //        async (ct) => await GetVideoAsync(videoName, ct),
        //        cancellationToken);

        //    var streamingToken = await GetStreamingTokenAsync(videoName, cancellationToken);
        //    var rtspTunnelUrl = video.Streaming.RtspTunnelUrl;

        //    if (shouldPlayback)
        //    {
        //        if (rtspTunnelUrl == null)
        //        {
        //            throw new InvalidOperationException($"Cannot get streaming URL for video {videoName}, video does not exist.");
        //        }

        //        playbackUrl = new Uri(rtspTunnelUrl);

        //        // Connect to Media Gateway
        //        await playbackDeviceTestClient.ConnectToGatewayInstanceAsync(
        //            new Uri(rtspTunnelUrl),
        //            streamingToken,
        //            cancellationToken);

        //        var rtspRetryPolicy = new RetryPolicyBuilder { NumberOfRetries = 30 }
        //        .RetryOnTransientErrors()
        //        .Build(TimeSpan.FromSeconds(30));

        //        var testRtspRequest = $"DESCRIBE rtsp://test.com/{livePipelineName}/rtspServerSink  RTSP/1.0\r\nCSeq: 1\r\n\r\n";

        //        var rtspResult = await rtspRetryPolicy.ExecuteAsync(async (ct) => await playbackDeviceTestClient.SendRtspRequestAsync(testRtspRequest, ct), cancellationToken);

        //        var rtspResponseCode = rtspResult.Split("\r\n")[0];
        //        if (!rtspResponseCode.Equals("RTSP/1.0 200 OK", StringComparison.OrdinalIgnoreCase))
        //        {
        //            throw new Exception("Playback is currently not streaming, validation failed.");
        //        }
        //    }
        //    else
        //    {
        //        if (rtspTunnelUrl != null)
        //        {
        //            throw new Exception("Video RTSP Playback URL found, validation failed.");
        //        }

        //        if (playbackUrl != null)
        //        {
        //            await Task.Delay(TimeSpan.FromSeconds(20).Milliseconds, cancellationToken); // delay to flush stream.

        //            try
        //            {
        //                await playbackDeviceTestClient.ConnectToGatewayInstanceAsync(
        //                    playbackUrl,
        //                    streamingToken,
        //                    cancellationToken);
        //            }
        //            catch (WebSocketException)
        //            {
        //                return playbackUrl;
        //            }

        //            throw new Exception("Playback is streaming, validation failed.");
        //        }
        //    }

        //    return playbackUrl;
        //}

        /// <inheritdoc/>
        public void UpdateTrackedCleanUpResources(
            IEnumerable<string>? videoNames = null,
            IEnumerable<string>? pipelineTopologyNames = null,
            IEnumerable<string>? livePipelineNames = null,
            IEnumerable<string>? edgeModuleNames = null)
        {
            foreach (var videoName in videoNames ?? Enumerable.Empty<string>())
            {
                _trackedVideoNamesForCleanup.Add(videoName);
            }

            foreach (var pipelineTopologyName in pipelineTopologyNames ?? Enumerable.Empty<string>())
            {
                _trackedPipelineTopologiesForCleanup.Add(pipelineTopologyName);
            }

            foreach (var livePipelineName in livePipelineNames ?? Enumerable.Empty<string>())
            {
                _trackedLivePipelineForCleanup.Add(livePipelineName);
            }
        }

        /// <inheritdoc />
        public async Task CleanupResourcesAsync(CancellationToken cancellationToken = default)
        {
            // Preserve Videos
            foreach (var videoName in _preservedVideoNames)
            {
                try
                {
                  Console.WriteLine(
                        $"Marking video {videoName} as preserved.");

                    var video = await _videosClient.Videos.GetAsync(
                        _accountResourceGroupName,
                        _accountName,
                        videoName,
                        cancellationToken);

                    video.Description = PreservedVideoDescription;

                    await _videosClient.Videos.UpdateAsync(
                        _accountResourceGroupName,
                        _accountName,
                        videoName,
                        video,
                        cancellationToken);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(
                        $"{ex}: Failed to preserve media account video: {0} while cleaning up...",
                        videoName);
                }
            }

            foreach (var videoName in _trackedVideoNamesForCleanup)
            {
                try
                {
                    await DeleteVideoAsync(videoName, cancellationToken);
                }
                catch (ErrorResponseException ex)
                {
                    if (ex.Response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine(
                        $"{ex}: Failed to delete video {0} while cleaning up...",
                            videoName);
                    }
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(
                         $"{ex}: Failed to delete video {0} while cleaning up...",
                        videoName);
                }
            }

            _trackedVideoNamesForCleanup?.Clear();

            foreach (var name in _trackedLivePipelineForCleanup)
            {
                try
                {
                    var livePipeline = await GetLivePipelineAsync(name, cancellationToken);

                    if (livePipeline == null)
                    {
                        continue;
                    }

                    if (livePipeline.State != LivePipelineState.Inactive)
                    {
                        await DeactivateLivePipelineAsync(name, cancellationToken);
                    }

                    await DeleteLivePipelineAsync(name, cancellationToken);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(
                         $"{ex}: Failed to delete live pipeline {0} while cleaning up...",
                        name);
                }
            }

            _trackedLivePipelineForCleanup.Clear();

            foreach (var name in _trackedPipelineTopologiesForCleanup)
            {
                try
                {
                    var pipelineTopology = await GetPipelineTopologyAsync(name, cancellationToken);

                    if (pipelineTopology != null)
                    {
                        await DeletePipelineTopologyAsync(name, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                         $"{ex}: Failed to delete pipeline topology {0} while cleaning up...",
                        name);
                }
            }

            _trackedPipelineTopologiesForCleanup.Clear();

            foreach (var name in _trackedAccessPolicyNamesForCleanup)
            {
                try
                {
                    await DeleteAccessPolicyAsync(name, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                         $"{ex}: Failed to delete access policy {0} while cleaning up...",
                        name);
                }
            }

            _trackedAccessPolicyNamesForCleanup.Clear();
        }

        ///// <inheritdoc />
        //public async Task<EdgeModuleEntity> CreateEdgeModuleAsync(string edgeModuleName, EdgeModuleEntity edgeModuleEntity, bool trackForCleanup = true, CancellationToken cancellationToken = default)
        //{
        //    Check.NotNullOrWhiteSpace(edgeModuleName, nameof(edgeModuleName));

        //    if (trackForCleanup)
        //    {
        //        _trackedEdgeModulesForCleanup.Add(edgeModuleName);
        //    }

        //    return await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
        //        async () => await _edgeModulesClient.EdgeModules.CreateOrUpdateAsync(
        //            _accountResourceGroupName,
        //            _accountName,
        //            edgeModuleName,
        //            cancellationToken));
        //}

        ///// <inheritdoc />
        //public async Task<EdgeModuleEntity> GetEdgeModuleAsync(string edgeModuleName, CancellationToken cancellationToken = default)
        //{
        //    return await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
        //        async () => await _edgeModulesClient.EdgeModules.GetAsync(
        //            _accountResourceGroupName,
        //            _accountName,
        //            edgeModuleName,
        //            cancellationToken));
        //}

        ///// <inheritdoc />
        //public async Task<List<EdgeModuleEntity>> GetAllEdgeModules(CancellationToken cancellationToken = default)
        //{
        //    return await ListAllEntitiesAsync(
        //        async () => await _edgeModulesClient.EdgeModules.ListAsync(_accountResourceGroupName, _accountName, cancellationToken: cancellationToken),
        //        async token => await _edgeModulesClient.EdgeModules.ListNextAsync(token, cancellationToken),
        //        cancellationToken: cancellationToken);
        //}

        ///// <inheritdoc />
        //public async Task DeleteEdgeModuleAsync(string edgeModuleName, CancellationToken cancellationToken = default)
        //{
        //    await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
        //        async () => await _edgeModulesClient.EdgeModules.DeleteAsync(
        //            _accountResourceGroupName,
        //            _accountName,
        //            edgeModuleName,
        //            cancellationToken));
        //}

        ///// <inheritdoc />
        //public async Task<string> ListProvisioningTokenAsync(string name, DateTimeOffset? expirationDate, CancellationToken cancellationToken = default)
        //{
        //    expirationDate ??= TimeProvider.UtcNow.AddDays(7);

        //    var tokenEntity = await GetTransientErrorRetryPolicy(cancellationToken).ExecuteAsync(
        //        async () => await _edgeModulesClient.EdgeModules.ListProvisioningTokenAsync(
        //            _accountResourceGroupName,
        //            _accountName,
        //            name,
        //            new ListProvisioningTokenInput(expirationDate.Value),
        //            cancellationToken));

        //    return tokenEntity.Token;
        //}

        ///// <inheritdoc />
        //public async Task<ManifestInfo?> GetVideoStreamingManifestAsync(string videoName, CancellationToken cancellationToken = default)
        //{
        //    var (streamingUrl, playbackToken) = await GetVideoArchiveStreamingUrlAndPlaybackTokenAsync(videoName, cancellationToken);

        //    return await StreamingManifestValidator.GetStreamingManifestAsync(streamingUrl, playbackToken, shouldBeStreaming: null, cancellationToken);
        //}

        /// <inheritdoc />
        public void Dispose()
        {
            _videosClient?.Dispose();
        }

        private static IAsyncPolicy GetTransientErrorRetryPolicy(CancellationToken cancellationToken = default)
        {
            return Policy.Handle<HttpRequestException>().RetryAsync(MaxRetries);
        }

        /// <summary>
        /// Pages through all the entities.
        /// </summary>
        /// <remarks>
        /// Copied from <c>/One/_git/AAPT-Media-Platform/src/Test/Feature/ArmMetadata/TestBase.cs</c>.
        /// </remarks>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="getFirstPageFunc">The function to get the first page.</param>
        /// <param name="getPageWithContinuationTokenFunc">The function to get subsequent pages given continuation token.</param>
        /// <param name="maxIterations">The max iterations for paginating. If all entities are not returned before that, the function will fail the test.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>A list of all entities of given type in database.</returns>
        private static async Task<List<T>> ListAllEntitiesAsync<T>(
            Func<Task<IPage<T>>> getFirstPageFunc,
            Func<string, Task<IPage<T>>> getPageWithContinuationTokenFunc,
            int maxIterations = 1000,
            CancellationToken cancellationToken = default)
        {
            var retryPolicy = GetTransientErrorRetryPolicy(cancellationToken);
            IPage<T> receivedEntities;
            try
            {
                receivedEntities = await retryPolicy.ExecuteAsync(async () => await getFirstPageFunc());
            }
            catch (RestException e)
            {
                // TODO: Remove this when we handle account not found
               Console.WriteLine("{e}: List Entities failed.");

                return new List<T>();
            }

            var entitiesToReturn = new List<T>(receivedEntities);
            var iteration = 0;

            while (receivedEntities.NextPageLink != null && iteration++ < maxIterations)
            {
                receivedEntities = await retryPolicy.ExecuteAsync(async () => await getPageWithContinuationTokenFunc(receivedEntities.NextPageLink));
                entitiesToReturn.AddRange(receivedEntities);
            }

            if (receivedEntities.NextPageLink != null)
            {
                var errorMessage = $"Exceeded max iterations - {maxIterations}";

                Console.WriteLine(errorMessage);

                throw new Exception(errorMessage);
            }

            return entitiesToReturn;
        }

        private async Task<(Uri StreamingUrl, string PlaybackToken)> GetVideoArchiveStreamingUrlAndPlaybackTokenAsync(string videoName, CancellationToken cancellationToken = default)
        {
            var retryPolicy = GetTransientErrorRetryPolicy(cancellationToken);

            var video = await retryPolicy.ExecuteAsync(
                async (ct) => await GetVideoAsync(videoName, ct),
                cancellationToken);

            if (video == null)
            {
                throw new InvalidOperationException($"Cannot get streaming URL for video {videoName}, video does not exist.");
            }

            if (video.Streaming.ArchiveBaseUrl == null)
            {
                throw new InvalidOperationException($"Cannot get streaming URL for video {videoName}, video archive URL does not exist.");
            }

            var playbackTokenResult = await _videosClient.Videos.ListStreamingTokenAsync(
                _accountResourceGroupName,
                _accountName,
                videoName,
                cancellationToken);

            return (new Uri($"{video.Streaming.ArchiveBaseUrl}/manifest"), playbackTokenResult.Token);
        }

        ///// <summary>
        ///// Preserve video for diagnostics.
        ///// </summary>
        ///// <param name="videoName">The name of the video to preserve.</param>
        ///// <param name="logManifest">Whether to log manifest.</param>
        ///// <param name="exception">Manifest invalid exception.</param>
        //private void PreserveMediaDiagnostics(string videoName, bool logManifest, ManifestInvalidException exception)
        //{
        //    _preservedVideoNames.Add(videoName);

        //    var messageBuilder = new StringBuilder();
        //    messageBuilder.AppendLine($"Video {videoName} is preserved for diagnostics.");

        //    if (logManifest)
        //    {
        //        var oldManifestContent = exception.PreviousManifest?.RawXmlString;
        //        var newManifestContent = exception.Manifest?.RawXmlString;

        //        if (!string.IsNullOrEmpty(oldManifestContent))
        //        {
        //            messageBuilder.AppendLine($"Old Manifest: {oldManifestContent}");
        //        }

        //        if (!string.IsNullOrEmpty(newManifestContent))
        //        {
        //            messageBuilder.AppendLine($"New Manifest: {newManifestContent}");
        //        }
        //    }

        //    Logger.LogWarning(
        //        LogCategory.TestCommon,
        //        EventId.TestCommonAmsArmClientInformation,
        //        messageBuilder.ToString());
        //}
    }
}
