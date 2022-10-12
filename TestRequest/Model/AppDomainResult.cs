using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestRequest.Model
{
    public class AppDomainResult
    {
        public bool Success { get; set; } = false;
        public object Data { get; set; }
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
    }
}
