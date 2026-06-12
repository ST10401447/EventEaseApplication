using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseApplication.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int EventID { get; set; }

        public int VenueID { get; set; }

        [ForeignKey(nameof(EventID))]
        public Event? Event { get; set; }

        [ForeignKey(nameof(VenueID))]
        public Venue? Venue { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
