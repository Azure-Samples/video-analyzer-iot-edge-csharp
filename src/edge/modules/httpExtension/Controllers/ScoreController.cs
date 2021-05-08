// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using HttpExtension.Processors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace HttpExtension.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    /// <summary>Class <c>ScoreController</c> is responsible for handling the http requests.
    /// </summary>
    public class ScoreController : ControllerBase
    {
        private readonly ILogger<ScoreController> logger;

        public ScoreController(ILogger<ScoreController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        /// <summary>This method handles the http request from AVA and returns an Inference result
        /// <returns>
        /// Http status 200: if the image was successfully processed a JSON representation of an Inference instance is returned.
        /// Http status 400: if there was an error processing the image.
        /// </returns>
        /// </summary>
        public async Task<IActionResult> ProcessImage()
        {
            try
            {
                logger.LogInformation("Process image request received.");

                var stream = new MemoryStream();
                await Request.Body.CopyToAsync(stream);

                Image image = Image.FromStream(stream);

                var imageProcessor = new ImageProcessor(logger);

                var response = imageProcessor.ProcessImage(image);

                return new ObjectResult(response);
            }
            catch(Exception ex)
            {
                var errorMessage = $"An error occurred processing request: {ex.Message}";
                logger.LogError(errorMessage);
                return new BadRequestObjectResult(errorMessage);
            }
        }
    }
}
