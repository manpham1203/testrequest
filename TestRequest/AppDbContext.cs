using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestRequest.Model;

namespace TestRequest
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Tour> Tour { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<Booking> Booking { get; set; }
    }
}
