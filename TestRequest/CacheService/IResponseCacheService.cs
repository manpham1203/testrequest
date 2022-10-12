using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestRequest.Model;

namespace TestRequest.Cache
{
    public interface IResponseCacheService
    {
        Task SetCacheResponseAsync(string cacheKey, object response, TimeSpan timeOut);
        Task<string> GetCachedResponseAsync(string cacheKey);
        Task<DataCacheBookingTour> GetBookingTicketCachedResponseAsync(string cacheKey);
        Task UpdateBookingTicketCachedResponseAsync(string cacheKey, object response, TimeSpan timeOut);
        Task RemoveCacheResponseAsync(string pattern);
    }
}
