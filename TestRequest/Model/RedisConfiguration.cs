using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestRequest.Model
{
    public class RedisConfiguration
    {
        public bool Enabled { get; set; }
        public string ConnectionString { get; set; }
    }
}
