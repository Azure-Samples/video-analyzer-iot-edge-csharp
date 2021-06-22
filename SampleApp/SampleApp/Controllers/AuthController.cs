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

namespace SampleApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IOptions<AuthModel> AuthSettings;
        private readonly IOptions<ClientModel> ClientSettings;

        public AuthController(ILogger<AuthController> logger, IOptions<AuthModel> authModel, IOptions<ClientModel> clientModel)
        {
            _logger = logger;
            AuthSettings = authModel;
            ClientSettings = clientModel;
        }

        [HttpGet]
        public IActionResult GetConfig()
        {
            var result = ClientSettings.Value;
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetToken()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://login.windows.net");
            var request = new HttpRequestMessage(HttpMethod.Post, $"/{AuthSettings.Value.TenantId}/oauth2/token");

            var keyValues = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("resource", "https://management.core.windows.net/"),
                new KeyValuePair<string, string>("client_id", AuthSettings.Value.ClientId),
                new KeyValuePair<string, string>("client_secret", AuthSettings.Value.ClientSecret),
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