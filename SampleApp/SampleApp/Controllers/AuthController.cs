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

namespace SampleApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration Configuration;

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetToken()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://login.windows.net");
            var request = new HttpRequestMessage(HttpMethod.Post, $"/{Configuration["Auth:tenantId"]}/oauth2/token");

            var keyValues = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("resource", "https://management.core.windows.net/"),
                new KeyValuePair<string, string>("client_id", Configuration["Auth:clientId"]),
                new KeyValuePair<string, string>("client_secret", Configuration["Auth:clientSecret"]),
                new KeyValuePair<string, string>("grant_type", "client_credentials") 
            };

            request.Content = new FormUrlEncodedContent(keyValues);
            var response = await client.SendAsync(request);

            JObject jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            if (response.IsSuccessStatusCode)
            {
                var accessToken = jsonResponse.SelectToken("access_token").ToString();
                return Ok(accessToken);
            }
            else
            {
                var errorDescription = jsonResponse.SelectToken("error_description").ToString();
                return BadRequest(errorDescription);
            }
        }
    }
}