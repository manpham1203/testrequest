using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestRequest.Cache;
using TestRequest.Model;

namespace TestRequest.AppAttribute
{
    public class CacheBookingAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _timeToLiveSeconds;
        public CacheBookingAttribute(int timeToLiveSeconds = 432000) // 5 Ngày
        {
            _timeToLiveSeconds = timeToLiveSeconds;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            RedisConfiguration redisConfiguration = context.HttpContext.RequestServices.GetRequiredService<RedisConfiguration>();
            if (!redisConfiguration.Enabled)
            {
                //nếu không cấu hình sử dụng cache => cho chạy middware tiếp theo (ở đây là chạy vô action để lấy data xong rồi return luôn)
                await next();
                return;
            }
            Booking data = (Booking)context.ActionArguments.Values.FirstOrDefault();

            string cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request, data);
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();

            lock (this)
            {
                DataCacheBookingTour cacheResponse = (DataCacheBookingTour)cacheService.GetBookingTicketCachedResponseAsync(cacheKey);

                if (cacheResponse != null)
                {
                    var numTicket = data.tickets.Count();
                    if (cacheResponse.total - cacheResponse.success - cacheResponse.waiting - numTicket < 0)
                    {
                        context.Result = new ContentResult
                        {
                            Content = "Vé không còn bà con ơi",
                            ContentType = "application/json",
                            StatusCode = 200
                        };
                        return;
                    }
                    else
                    {
                        // nếu còn đủ vé thì tăng cache lên
                        cacheResponse.waiting = cacheResponse.waiting + numTicket;
                        cacheService.UpdateBookingTicketCachedResponseAsync(cacheKey, cacheResponse, TimeSpan.FromSeconds(_timeToLiveSeconds));
                    }
                }
                var excutedContext = next();
                if ((OkObjectResult)excutedContext.Result.Result is OkObjectResult objectResult)
                {
                    cacheService.SetCacheResponseAsync(cacheKey, objectResult.Value, TimeSpan.FromSeconds(_timeToLiveSeconds));
                }
            }


            //nếu không có cache => lấy data => gán vô cache
        }
        /// <summary>
        /// Tạo CacheKey theo request
        /// </summary>
        private static string GenerateCacheKeyFromRequest(HttpRequest request, Booking obj)
        {
            StringBuilder keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");
            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }
            keyBuilder.Append(obj.tourId);
            return keyBuilder.ToString();
        }
    }
}
