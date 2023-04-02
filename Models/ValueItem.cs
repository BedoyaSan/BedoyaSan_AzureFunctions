using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedoyaSan_AzureFunctions.Models
{
    public class ValueItem : ITableEntity
    {
        /// <summary>
        /// Name code for the Entity
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Integer value
        /// </summary>
        public int Value { get; set; }

        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;

        public ValueItem()
        {

        }
    }
}
