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
using Microsoft.Azure.Devices;
using Azure.Media.VideoAnalyzer.Edge.Models;
using SampleApp.Helpers;

namespace SampleApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class VideoAnalyzerController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly VideoAnalyzerClientConfiguration VideoAnalyzerClientConfiguration;
        private readonly ServiceClient EdgeClient;

        public VideoAnalyzerController(ILogger<AuthController> logger, IOptions<VideoAnalyzerClientConfiguration> clientConfiguration)
        {
            _logger = logger;
            VideoAnalyzerClientConfiguration = clientConfiguration.Value;
            EdgeClient = ServiceClient.CreateFromConnectionString(VideoAnalyzerClientConfiguration.IotHubConnectionString);
        }

        [HttpGet]
        public async Task<IActionResult> GetToken()
        {
            try
            {
                
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
    }
}