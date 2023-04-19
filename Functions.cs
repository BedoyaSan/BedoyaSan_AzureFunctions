using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using BedoyaSan_AzureFunctions.Models;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Primitives;
using Azure;

namespace BedoyaSan_AzureFunctions
{
    public static class Functions
    {
        /// <summary>
        /// This function keeps track of the number of times it has been invoked, which can be used to track the number of visitors to a website.
        /// It stores the Public IP Address obtained from the request headers, to check for only unique visitor counts.
        /// If the table or entity does not exist, it will be created upon first invocation.
        /// </summary>
        /// <param name = "req" ></ param >
        /// < param name= "log" ></ param >
        /// < returns ></ returns >
        [FunctionName("visitorcount")]
        public static async Task<IActionResult> VisitorCount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            TableServiceClient tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("CosmosDBConnectionString"));
            TableClient tableClient = tableServiceClient.GetTableClient("bedoyasan");
            await tableClient.CreateIfNotExistsAsync();

            bool newVisit = true;

            IPAddress ipAddress = null;
            if (req.Headers.TryGetValue("X-Forwarded-For", out StringValues values))
            {
                var ipn = values.FirstOrDefault().Split(new char[] { ',' }).FirstOrDefault().Split(new char[] { ':' }).FirstOrDefault();
                IPAddress.TryParse(ipn, out ipAddress);
            }

            if (ipAddress != null)
            {
                TableEntity visitor = tableClient.Query<TableEntity>(item => item.PartitionKey.Equals("visitorinfo") && item.RowKey.Equals(ipAddress.ToString())).FirstOrDefault();

                if (visitor == null)
                {
                    visitor = new TableEntity();
                    visitor.PartitionKey = "visitorinfo";
                    visitor.RowKey = ipAddress.ToString();
                    visitor.Timestamp = DateTime.UtcNow.AddHours(-5);

                    await tableClient.UpsertEntityAsync(visitor);
                }
                else
                {
                    newVisit = false;
                }
            }
            else
            {
                log.LogInformation("There was no ipAddress from the headers, counting as a new visitor");
            }

            ValueItem itemEntity = tableClient.Query<ValueItem>(item => item.PartitionKey.Equals("generaldata") && item.RowKey.Equals("visitorcount")).FirstOrDefault();

            if (itemEntity == null)
            {
                itemEntity = new ValueItem();
                itemEntity.PartitionKey = "generaldata";
                itemEntity.RowKey = "visitorcount";
                itemEntity.Name = "Count";
                itemEntity.Value = 1;
            }
            else if (newVisit)
            {
                itemEntity.Value += 1;
            }

            await tableClient.UpsertEntityAsync(itemEntity);

            ResponseValue response = new ResponseValue(newVisit ? "Success, first time" : "Success, recurrent visitor", itemEntity.Value);

            return new OkObjectResult(response);
        }

        /// <summary>
        /// This function will run every monday, and will delete the visitor registers from the table visitorinfo
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="log"></param>
        [FunctionName("visitorcleaner")]
        public static void VisitorCleaner(
            [TimerTrigger("0 0 * * mon")] TimerInfo timer,
            ILogger log)
        {
            TableServiceClient tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("CosmosDBConnectionString"));
            TableClient tableClient = tableServiceClient.GetTableClient("bedoyasan");
            tableClient.CreateIfNotExists();

            DateTime currentDate = DateTime.UtcNow.AddHours(-5);

            Pageable<ValueItem> visitorInfo = tableClient.Query<ValueItem>(item => item.PartitionKey.Equals("visitorinfo"));

            foreach (ValueItem item in visitorInfo)
            {
                tableClient.DeleteEntity("visitorinfo", item.RowKey);
            }
        }
    }
}
