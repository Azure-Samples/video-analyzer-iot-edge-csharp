// -----------------------------------------------------------------------
//  <copyright company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using Azure.Media.VideoAnalyzer.Edge.Models;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SampleApp.Helpers;
using SampleApp.Hubs;
using SampleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Produces("application/json")]
    public class VideoAnalyzerController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly VideoAnalyzerClientConfiguration VideoAnalyzerClientConfiguration;
        private readonly ServiceClient EdgeClient;
        private readonly IHubContext<EventHub> EventHub;

        public VideoAnalyzerController(ILogger<AuthController> logger, IOptions<VideoAnalyzerClientConfiguration> clientConfiguration, IHubContext<EventHub> eventHub)
        {
            _logger = logger;
            VideoAnalyzerClientConfiguration = clientConfiguration.Value;
            EdgeClient = ServiceClient.CreateFromConnectionString(VideoAnalyzerClientConfiguration.IotHubConnectionString);
            EventHub = eventHub;
        }

        #region PipelineTopologies
        [HttpGet]
        public async Task<IActionResult> PipelineTopologyList()
        {
            try
            {
                var method = new PipelineTopologyListRequest();
                var result = await InvokeDirectMethodHelper(method);
                var response = DeserializeResult<List<PipelineTopology>>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PipelineTopologyGet([FromQuery] string pipelineTopologyName)
        {
            try
            {
                var method = new PipelineTopologyGetRequest(pipelineTopologyName);
                var result = await InvokeDirectMethodHelper(method);

                if (result.Status >= 200 && result.Status < 400)
                {
                    var response = DeserializeResult<PipelineTopology>(result);
                    return Ok(response);
                }
                else
                {
                    return StatusCode(result.Status);
                }    
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PipelineTopologySet([FromQuery] string pipelineTopologyName)
        {
            try
            {
                var pipelineTopology = Builder.BuildPipelineTopology(pipelineTopologyName);
                var method = new PipelineTopologySetRequest(pipelineTopology);
                var result = await InvokeDirectMethodHelper(method);

                if (result.Status >= 200 && result.Status < 400)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.GetPayloadAsJson());
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> PipelineTopologyDelete([FromQuery]string pipelineTopologyName)
        {
            try
            {
                var method = new PipelineTopologyDeleteRequest(pipelineTopologyName);
                var result = await InvokeDirectMethodHelper(method);

                if (result.Status >= 200 && result.Status < 400)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.GetPayloadAsJson());
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }
        #endregion

        #region LivePipelines
        [HttpGet]
        public async Task<IActionResult> LivePipelineList()
        {
            try
            {
                var method = new LivePipelineListRequest();
                var result = await InvokeDirectMethodHelper(method);
                JObject response = JsonConvert.DeserializeObject<JObject>(result.GetPayloadAsJson());

                var value = response.SelectToken("value");

               // var response = DeserializeResult<List<LivePipeline>>(result);
                return Ok(value.ToString());
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpPut]
        public async Task<IActionResult> LivePipelineSet([FromBody] LivePipelineRequestPayload requestPayload)
        {
            try
            {
                var livePipeline = Builder.BuildLivePipepine(requestPayload.LivePipelineName, requestPayload.PipelineTopologyName,
                    requestPayload.Url, requestPayload.Username, requestPayload.Password, requestPayload.VideoName);
                var method = new LivePipelineSetRequest(livePipeline);
                var result = await InvokeDirectMethodHelper(method);

                if (result.Status >= 200 && result.Status < 400)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.GetPayloadAsJson());
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpGet]
        public async Task<IActionResult> LivePipelineGet([FromQuery] string livePipelineName)
        {
            try
            {
                var method = new LivePipelineGetRequest(livePipelineName);
                var result = await InvokeDirectMethodHelper(method);

                if (result.Status >= 200 && result.Status < 400)
                {
                    var response = DeserializeResult<LivePipeline>(result);
                    return Ok(response);
                }
                else
                {
                    return StatusCode(result.Status);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetModuleLogs([FromQuery] string livePipelineName)
        {
            try
            {
                var method = new LivePipelineGetRequest(livePipelineName);
                var result = await InvokeDirectMethodHelper(method);

                if (result.Status >= 200 && result.Status < 400)
                {
                    var response = DeserializeResult<LivePipeline>(result);
                    return Ok(response);
                }
                else
                {
                    return StatusCode(result.Status);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> LivePipelineDelete([FromQuery] string livePipelineName)
        {
            try
            {
                var method = new LivePipelineDeleteRequest(livePipelineName);
                var result = await InvokeDirectMethodHelper(method);

                if (result.Status >= 200 && result.Status < 400)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.GetPayloadAsJson());
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpPost]
        public async Task<IActionResult> LivePipelineActivate([FromQuery] string livePipelineName)
        {
            try
            {
                var method = new LivePipelineActivateRequest(livePipelineName);
                var result = await InvokeDirectMethodHelper(method);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }

        [HttpPost]
        public async Task<IActionResult> LivePipelineDeactivate([FromQuery] string livePipelineName)
        {
            try
            {
                var method = new LivePipelineDeactivateRequest(livePipelineName);
                var result = await InvokeDirectMethodHelper(method);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
        }


        [HttpGet]
        public IActionResult StopListeningToEvents()
        {
            AppConstants.TokenSource.Cancel();
            return StatusCode(204);
        }

        [HttpGet]
        public async Task<IActionResult> ListenToEvents()
        {
            AppConstants.TokenSource = new CancellationTokenSource();
            var consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            var consumer = new EventHubConsumerClient(
            consumerGroup,
            VideoAnalyzerClientConfiguration.BuiltinEndpoint,
            VideoAnalyzerClientConfiguration.IotHubName);

            try
            {
                string firstPartition = (await consumer.GetPartitionIdsAsync(AppConstants.TokenSource.Token)).Last();
                PartitionProperties properties = await consumer.GetPartitionPropertiesAsync(firstPartition, AppConstants.TokenSource.Token);
                EventPosition startingPosition = EventPosition.FromSequenceNumber(properties.LastEnqueuedSequenceNumber);

                await foreach (PartitionEvent partitionEvent in consumer.ReadEventsFromPartitionAsync(
                    firstPartition,
                    startingPosition,
                    AppConstants.TokenSource.Token))
                {
                    await EventHub.Clients.All.SendAsync("ReceivedNewEvent", partitionEvent.Data.EventBody.ToString());
                }
                return StatusCode(204);
            }
            catch (TaskCanceledException)
            {
                // This is expected if the cancellation token is
                // signaled.
                return StatusCode(204);
            }
            finally
            {
                await consumer.CloseAsync();
            }
        }
        #endregion

        private async Task<CloudToDeviceMethodResult> InvokeDirectMethodHelper(MethodRequest request)
        {
            var directMethod = new CloudToDeviceMethod(request.MethodName);
            directMethod.SetPayloadJson(request.GetPayloadAsJson());

            return await EdgeClient.InvokeDeviceMethodAsync(VideoAnalyzerClientConfiguration.DeviceId, VideoAnalyzerClientConfiguration.ModuleId, directMethod);
        }

        private T DeserializeResult<T>(CloudToDeviceMethodResult result)
        {
            JObject response = JsonConvert.DeserializeObject<JObject>(result.GetPayloadAsJson());

            if (response == null)
            {
                return default(T);
            }

            var value = response.SelectToken("value");

            if(value != null)
                return JsonConvert.DeserializeObject<T>(value.ToString());
            else
                return JsonConvert.DeserializeObject<T>(response.ToString());

        }
    }
}