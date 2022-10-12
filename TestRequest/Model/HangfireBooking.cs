using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TestRequest.Model
{
    public class HangfireBooking
    {
        /// <summary>
        /// số người đặt cùng lúc
        /// </summary>
        [DefaultValue(20)]
        public int? totalBooking { get; set; }

        /// <summary>
        /// số vé của mỗi người
        /// </summary>
        [DefaultValue(2)]
        public int? totalTicketPerPerson { get; set; }
        /// <summary>
        /// tourId
        /// </summary>
        public int tourId { get; set; }
    }
}
