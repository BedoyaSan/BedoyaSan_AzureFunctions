using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Azure;
using System.Collections.Concurrent;
using BedoyaSan_AzureFunctions.Models;
using System.Linq;

namespace BedoyaSan_AzureFunctions
{
    public static class Functions
    {
        /// <summary>
        /// This function keeps track of the number of times it has been invoked, which can be used to track the number of visitors to a website.
        /// If the table or entity does not exist, it will be created upon first invocation.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("visitorcount")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            TableServiceClient tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("CosmosDBConnectionString"));
            TableClient tableClient = tableServiceClient.GetTableClient("bedoyasan");

            await tableClient.CreateIfNotExistsAsync();

            ValueItem itemEntity = tableClient.Query<ValueItem>(item => item.PartitionKey.Equals("generaldata") && item.RowKey.Equals("visitorcount")).FirstOrDefault();

            if(itemEntity == null)
            {
                itemEntity = new ValueItem();
                itemEntity.PartitionKey = "generaldata";
                itemEntity.RowKey = "visitorcount";
                itemEntity.Name = "Count";
                itemEntity.Value = 1;
            }
            else
            {
                itemEntity.Value += 1;
            }

            await tableClient.UpsertEntityAsync(itemEntity);

            ResponseValue response = new ResponseValue("Success", itemEntity.Value);

            return new OkObjectResult(response);
        }
    }
}
