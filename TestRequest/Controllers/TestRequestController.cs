using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestRequest.Model.TestRequest;

namespace TestRequest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestRequestController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post([FromBody] PostRequest request)
        {
            for (int i = 0; i < request.totalRequest; i++)
            {
                BackgroundJob.Schedule(
                () => this.PostRequest(request),
                DateTime.UtcNow.AddHours(2));
            }

            return Ok("thành công");
        }
        [HttpGet]
        public IActionResult Get([FromBody] GetRequest request)
        {
            for (int i = 0; i < request.totalRequest; i++)
            {
                BackgroundJob.Schedule(
                () => this.GetRequest(request),
                DateTime.UtcNow.AddHours(2));
            }
            return Ok("thành công");
        }
        [NonAction]
        public async Task PostRequest(PostRequest request)
        {
            try
            {
                HttpContent content = new StringContent(request.content, Encoding.UTF8, request.contentType);

                using HttpClient client = new HttpClient();
                foreach (var header in request.headers)
                {
                    client.DefaultRequestHeaders.Add(header.name, header.value);
                }

                await client.PostAsync(request.url, content);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        [NonAction]
        public async Task GetRequest(GetRequest request)
        {
            try
            {
                using HttpClient client = new HttpClient();
                foreach (var header in request.headers)
                {
                    client.DefaultRequestHeaders.Add(header.name, header.value);
                }

                await client.GetAsync(request.url);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
