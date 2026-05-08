using System.ComponentModel.DataAnnotations;

namespace EventEaseApplication.Models
{
    public class VenueType
    {
        [Key]
        public int VenueTypeID { get; set; }

        [Required]
        public string VenueTitle { get; set; }

        public ICollection<Venue>? Venues { get; set; }
    }
}
