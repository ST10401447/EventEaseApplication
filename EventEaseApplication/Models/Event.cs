using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseApplication.Models
{
    public class Event
    {

        [Key]
        public int EventID { get; set; }

        [Required]
        public string EventName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public int EventTypeID { get; set; }

        [ForeignKey(nameof(EventTypeID))]
        public EventType? EventType { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
    }
}
