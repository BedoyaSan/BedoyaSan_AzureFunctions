using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedoyaSan_AzureFunctions.Models
{
    /// <summary>
    /// Response model for Status and Value
    /// </summary>
    public class ResponseValue
    {
        /// <summary>
        /// Status of the operation
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Integer value of the operation
        /// </summary>
        public int Value { get; set; }

        public ResponseValue() { }
        public ResponseValue(string status, int value) { Status = status; Value = value; }
    }
}
