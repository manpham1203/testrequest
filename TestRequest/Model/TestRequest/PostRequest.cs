using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TestRequest.Model.TestRequest
{
    public class PostRequest : BaseRequest
    {
        /// <summary>
        /// dữ liệu
        /// </summary>
        public string content { get; set; }
        public string contentType { get; set; }

    }
}
