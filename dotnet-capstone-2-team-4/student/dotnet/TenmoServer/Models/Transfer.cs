using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TenmoServer.Models
{
    public class Transfer
    {
        public int? Id { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }


    }


}

