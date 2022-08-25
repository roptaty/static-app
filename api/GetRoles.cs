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
            public string AccessToken { get; set; }
        }

        class RolesResult
        {
            [System.Text.Json.Serialization.JsonPropertyName("roles")]
            public string[] Roles { get; set; }

        }


        [FunctionName("GetRoles")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetRoles are triggered");
            List<string> roles = new List<string>();
            using StreamReader sr = new StreamReader(req.Body);
            var requestBody = System.Text.Json.JsonSerializer.Deserialize<RequestBody>(sr.ReadToEnd());

            if (string.IsNullOrEmpty(requestBody?.AccessToken))
            {
                log.LogInformation("Missing access token");
                return new JsonResult(roles);
            }


            try
            {
                // parse the body of req
                // get the accessTOken
                string accessToken = requestBody.AccessToken;


                var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (request) =>
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    await Task.FromResult<object>(null);
                }));

                var pageCollection = await graphClient.Me.GetMemberGroups().Request().Filter($"$id eq {PEDIA_GROUP_ID}").PostAsync();
                if (pageCollection.Any())
                {
                    log.LogInformation("Adding pedia");
                    roles.Add(ROLE_NAME);
                }
                else
                {
                    log.LogInformation("No membership...");
                }
            }
            catch (Exception ex)
            {
                log.LogInformation("Exception when querying for groups: " + ex.Message);
            }

            return new JsonResult(new RolesResult { Roles = roles.ToArray() });
        }


    }


}
