using EventEaseApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEaseApplication.Data
{
    public class ApplicationDbContext:DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        public DbSet<EventType> EventTypes { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<VenueType> VenueTypes { get; set; }

        public DbSet<Venue> Venues { get; set; }

        public DbSet<Booking> Bookings { get; set; }
    }
}
