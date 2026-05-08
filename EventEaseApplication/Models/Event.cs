using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseApplication.Models
{
    public class Event
    {

        [Key]
        public int EventID { get; set; }

        [Required(ErrorMessage = "Event Name is required")]
        public string EventName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Event Date is required")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Please select an Event Type")]
        public int EventTypeID { get; set; }

        [ForeignKey(nameof(EventTypeID))]
        public EventType? EventType { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
    }
}
