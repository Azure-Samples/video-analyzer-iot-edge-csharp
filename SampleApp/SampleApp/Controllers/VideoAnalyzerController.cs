// -----------------------------------------------------------------------
//  <copyright company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using SampleApp.Models;
using Microsoft.Extensions.Options;
using Microsoft.Media.LiveVideoAnalytics.Apis.Client.MediaGraphManager.RP20210501privatepreview;
using Microsoft.Media.LiveVideoAnalytics.Apis.Client.MediaGraphManager.RP20210501privatepreview.Models;
using SampleApp.Helpers;

namespace SampleApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class VideoAnalyzerController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IOptions<VideoAnalyzerClientConfiguration> VideoAnalyzerClientConfiguration;
        private readonly VideoAnalyzerClient VideoAnalyzerClient;


        public VideoAnalyzerController(ILogger<AuthController> logger, IOptions<VideoAnalyzerClientConfiguration> clientConfiguration)
        {
            _logger = logger;
            VideoAnalyzerClientConfiguration = clientConfiguration;
            VideoAnalyzerClient = VideoAnalyzerClientFactory.CreateAsync(clientConfiguration.Value).Result;
        }

        [HttpGet]
        public async Task<IActionResult> GetToken()
        {
            try
            {
                var response = await VideoAnalyzerClient.GetAllPipelineTopologyAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
    }
}