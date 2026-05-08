using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseApplication.Data;
using EventEaseApplication.Models;

namespace EventEaseApplication.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings

        // GET: Bookings
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var allBookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Event.EventType)
                .Include(b => b.Venue)
                .ToListAsync();

            if (string.IsNullOrWhiteSpace(searchString))
            {
                return View(allBookings.OrderByDescending(b => b.StartDate).ToList());
            }

            List<Booking> filteredBookings = new List<Booking>();
            string searchLower = searchString.Trim().ToLower();

            foreach (var booking in allBookings)
            {
                bool isMatch = false;

                if (booking.StartDate.ToString("dd MMM yyyy").ToLower().Contains(searchLower) ||
                    booking.StartDate.ToString("dd MMMM yyyy").ToLower().Contains(searchLower) ||
                    booking.StartDate.ToString("yyyy-MM-dd").Contains(searchString) ||
                    booking.EndDate.ToString("dd MMM yyyy").ToLower().Contains(searchLower) ||
                    booking.EndDate.ToString("dd MMMM yyyy").ToLower().Contains(searchLower) ||
                    (booking.Event?.EventName != null && booking.Event.EventName.ToLower().Contains(searchLower)) ||
                    (booking.Event?.EventType?.EventTitle != null && booking.Event.EventType.EventTitle.ToLower().Contains(searchLower)) ||
                    (booking.Venue?.VenueName != null && booking.Venue.VenueName.ToLower().Contains(searchLower)))
                {
                    isMatch = true;
                }

                if (isMatch)
                {
                    filteredBookings.Add(booking);
                }
            }

            return View(filteredBookings.OrderByDescending(b => b.StartDate).ToList());
        }


        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["EventID"] = new SelectList(_context.Events, "EventID", "Description");
            ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "VenueName");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,StartDate,EndDate,EventID,VenueID")] Booking booking)
        {

            if (ModelState.IsValid)
            {
                // ==================== STRONGER OVERLAP CHECK ====================
                bool isOverlapping = await _context.Bookings
                    .AnyAsync(b => b.VenueID == booking.VenueID &&
                                  // Check if time periods overlap
                                  b.StartDate < booking.EndDate &&
                                  b.EndDate > booking.StartDate);

                if (isOverlapping)
                {
                    ModelState.AddModelError("", "This venue is already booked for the selected time period. Please choose a different time or venue.");

                    ViewData["EventID"] = new SelectList(_context.Events, "EventID", "EventName", booking.EventID);
                    ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "VenueName", booking.VenueID);
                    return View(booking);
                }
                // =================================================================

                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventID"] = new SelectList(_context.Events, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "VenueName", booking.VenueID);
            return View(booking);
        }

         // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null)
            {
                return NotFound();
            }

            ViewData["EventID"] = new SelectList(_context.Events, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "VenueName", booking.VenueID);

            return View(booking);
        }

        


        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingID,StartDate,EndDate,EventID,VenueID")] Booking booking)
        {
            if (id != booking.BookingID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                bool isOverlapping = await _context.Bookings
                    .AnyAsync(b => b.VenueID == booking.VenueID &&
                                  b.BookingID != booking.BookingID &&     // Exclude current booking
                                  b.StartDate < booking.EndDate &&
                                  b.EndDate > booking.StartDate);

                if (isOverlapping)
                {
                    ModelState.AddModelError("", "This venue is already booked for the selected time period.");
                    ViewData["EventID"] = new SelectList(_context.Events, "EventID", "EventName", booking.EventID);
                    ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "VenueName", booking.VenueID);
                    return View(booking);
                }

                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventID"] = new SelectList(_context.Events, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "VenueName", booking.VenueID);
            return View(booking);
        }


            // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null) return NotFound();

            // Block past bookings
            if (booking.StartDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "You cannot delete a past booking that has already occurred.";
                return RedirectToAction(nameof(Index));   
            }
           
            return View(booking);   
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {

                // Double check to Prevent deleting past bookings
                if (booking.StartDate <= DateTime.Now)
                {
                    TempData["ErrorMessage"] = "You cannot delete a past booking.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingID == id);
        }
    }
}
