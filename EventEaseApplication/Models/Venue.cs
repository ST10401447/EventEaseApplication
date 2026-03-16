using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseApplication.Models
{
    public class Venue
    {
        [Key]
        public int VenueID { get; set; }

        [Required]
        public string VenueName { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public string ImageURL { get; set; }

        public int VenueTypeID { get; set; }

        [ForeignKey(nameof(VenueTypeID))]
        public VenueType? VenueType { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
    }
}
