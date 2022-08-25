using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.Linq;
using Microsoft.Graph;

namespace Waaler.Functions
{
    public static class GetRoles
    {
        public static string PEDIA_GROUP_ID = "bdc6799a-489f-4dfb-978a-0026190ddafd";
        public static string ROLE_NAME = "pedia";
        class RequestBody 
        {
            [System.Text.Json.Serialization.JsonPropertyName("accessToken")]
            public string AccessToken { get; set;}
        }

        [FunctionName("GetRoles")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            List<string> roles = new List<string>();
            using StreamReader sr = new StreamReader(req.Body);
            var requestBody = System.Text.Json.JsonSerializer.Deserialize<RequestBody>(sr.ReadToEnd());

            if (string.IsNullOrEmpty(requestBody?.AccessToken))
            {
                log.LogInformation("Missing access token");
                return new JsonResult(roles);
            }

            
            
            // parse the body of req
            // get the accessTOken
            string accessToken = requestBody.AccessToken;


            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (request) => {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                await Task.FromResult<object>(null);
            }));

            DirectoryObject directoryObject = await graphClient.Me.TransitiveMemberOf[PEDIA_GROUP_ID].Request().GetAsync();
            if (directoryObject != null) 
            {
                roles.Add(ROLE_NAME);
            }


            return new JsonResult(roles); 
        }

        
    }


}
