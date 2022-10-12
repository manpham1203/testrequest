using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestRequest.AppAttribute;
using TestRequest.Cache;
using TestRequest.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;

namespace TestRequest.Controllers
{
    /// <summary>
    /// đặt vé
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private AppDbContext _context;
        private IResponseCacheService responseCacheService;
        private IWebHostEnvironment iwebHostEnvironment;
        public BookingController(AppDbContext context, IServiceProvider serviceProvider, IWebHostEnvironment _iwebHostEnvironment)
        {
            _context = context;
            responseCacheService = serviceProvider.GetService<IResponseCacheService>();
            iwebHostEnvironment = _iwebHostEnvironment;
        }
        ///// <summary>
        ///// đặt cùng lúc (sau đó dùng hangfire để điều khiển)
        ///// </summary>
        //[HttpPost("booking-cache")]
        //public IActionResult Booking(Booking request)
        //{
        //    BackgroundJob.Schedule(
        //    () => this.ScheduleBooking(request),
        //    DateTime.UtcNow.AddHours(2));
        //    return Ok();
        //}



        /// <summary>
        /// tạo tour
        /// </summary>
        [HttpPost("create-tour")]
        public async Task<IActionResult> CreateTour(Tour request)
        {

            await _context.Tour.AddAsync(request);
            await _context.SaveChangesAsync();
            return Ok();
        }
        /// <summary>
        /// danh sách tour
        /// </summary>
        [HttpGet("get-tour")]
        //[CacheGet]
        public async Task<IActionResult> GetTour()
        {
            return Ok(await this._context.Tour.ToListAsync());
        }
        /// <summary>
        /// danh sách vé
        /// </summary>
        [HttpGet("get-ticket")]
        //[CacheGet]
        public async Task<IActionResult> GetTicket()
        {
            var data = await this._context.Ticket.ToListAsync();
            return Ok(new { totalItem = data.Count(), items = data });
        }
        /// <summary>
        /// xóa vé
        /// </summary>
        [HttpDelete("delete-all-ticket")]
        public async Task<IActionResult> DeleteTicket()
        {
            using (var transaction = await this._context.Database.BeginTransactionAsync())
            {
                try
                {
                    var booking = await this._context.Booking.ToListAsync();
                    this._context.Booking.RemoveRange(booking);
                    await this._context.SaveChangesAsync();
                    var ticket = await this._context.Ticket.ToListAsync();
                    this._context.Ticket.RemoveRange(ticket);
                    await this._context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Ok("thành công");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Lỗi");
                }
            }
        }
        [NonAction]
        public async Task<IActionResult> ScheduleBooking(Booking request)
        {
            DataCacheBookingTour dataCacheBookingTour = new DataCacheBookingTour();
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var tour = await _context.Tour.SingleOrDefaultAsync(x => x.id == request.tourId);
                    if (tour == null)
                    {
                        return BadRequest("tour không tồn tại");
                    }
                    var tickets = await _context.Ticket.Where(x => x.tourId == tour.id && (x.status == 1 || x.status == 2)).ToListAsync();
                    if (tour.slot - tickets.Count <= 0)
                    {
                        return BadRequest("hết chỗ");
                    }
                    if (tour.slot - tickets.Count < request.tickets.Count)
                    {
                        return BadRequest("không đủ chỗ");
                    }
                    await _context.Booking.AddAsync(request);
                    await _context.SaveChangesAsync();
                    foreach (var ticket in request.tickets)
                    {
                        ticket.bookingId = request.id;
                        ticket.status = 1;
                        ticket.tourId = tour.id;
                    }
                    await _context.Ticket.AddRangeAsync(request.tickets);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Đếm lại số lượng vé
                    var total = tour.slot;
                    var waiting = await _context.Ticket.Where(x => x.tourId == tour.id && x.status == 1).ToListAsync();
                    var success = await _context.Ticket.Where(x => x.tourId == tour.id && x.status == 2).ToListAsync();


                    dataCacheBookingTour.total = total;
                    dataCacheBookingTour.waiting = waiting.Count();
                    dataCacheBookingTour.success = success.Count();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return BadRequest("lỗi");
                }
            }
            return Ok(new AppDomainResult()
            {
                ResultCode = (int)HttpStatusCode.OK,
                ResultMessage = "thành công!",
                Data = dataCacheBookingTour,
                Success = true
            });
        }
        /// <summary>
        /// đặt vé bình thường
        /// </summary>
        [HttpPost("booking")]
        [CacheBooking]
        public async Task<IActionResult> CreateBooking(Booking request)
        {
            Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fffffff"));
            return await this.ScheduleBooking(request);
        }

        [NonAction]
        public async Task TestCache(string body)
        {
            string baseUrl = "http://localhost:5000/api/Booking/booking";

            HttpContent c = new StringContent(body, Encoding.UTF8, "application/json");
            try
            {
                using HttpClient client = new HttpClient();
                await client.PostAsync(baseUrl, c);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        /// <summary>
        /// mô phỏng đặt vé cùng lúc
        /// </summary>
        /// <param name="booking"></param>
        [HttpPost("hangfire-schedule")]
        public string HangfireSchedule([FromQuery] HangfireBooking booking)
        {
            var data = new Booking()
            {
                tourId = booking.tourId,
                tickets = new List<Ticket>()
            };
            for (int i = 0; i < booking.totalTicketPerPerson; i++)
            {
                data.tickets.Add(new Ticket());
            }
            string body = JsonSerializer.Serialize(data);
            for (int i = 0; i < booking.totalBooking; i++)
            {
                BackgroundJob.Schedule(
                () => this.TestCache(body),
                DateTime.UtcNow.AddHours(2));
            }
            var currentLinkSite = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/hangfire/jobs/scheduled?from=0&count={booking.totalBooking}";
            return $"{currentLinkSite}\nxong rồi, giờ vô hangfire dashboard (link trên) để chạy các schedule";
        }
    }
}
