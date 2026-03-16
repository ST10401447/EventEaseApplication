using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace EventEaseApplication.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeID { get; set; }

        [Required]
        public string EventTitle { get; set; }

        public ICollection<Event>? Events { get; set; }
    }
}
