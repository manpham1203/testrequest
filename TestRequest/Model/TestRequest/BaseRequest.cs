using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TestRequest.Model.TestRequest
{
    public class BaseRequest
    {
        public string url { get; set; }
        /// <summary>
        /// số lượng request
        /// </summary>
        [DefaultValue(1)]
        public int totalRequest { get; set; }
        
        /// <summary>
        /// header
        /// </summary>
        public List<Header> headers { get; set; }
    }
}
