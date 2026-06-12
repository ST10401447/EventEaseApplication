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

        // GET: Bookings - Advanced Filter (Venue Availability)
        public async Task<IActionResult> Index(string searchString, string status, string eventType, DateTime? startDateFrom, DateTime? endDateTo)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;           // Venue Availability
            ViewData["CurrentEventType"] = eventType;
            ViewData["StartDateFrom"] = startDateFrom?.ToString("yyyy-MM-dd");
            ViewData["EndDateTo"] = endDateTo?.ToString("yyyy-MM-dd");

            var query = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Event!.EventType)
                .Include(b => b.Venue)
                .AsQueryable();

            // Text Search
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string s = searchString.Trim().ToLower();
                query = query.Where(b =>
                    (b.Event != null && b.Event.EventName != null && b.Event.EventName.ToLower().Contains(s)) ||
                    (b.Venue != null && b.Venue.VenueName != null && b.Venue.VenueName.ToLower().Contains(s)) ||
                    (b.Event != null && b.Event.EventType != null && b.Event.EventType.EventTitle != null &&
                     b.Event.EventType.EventTitle.ToLower().Contains(s)));
            }

            // ==================== VENUE AVAILABILITY FILTER ====================
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Venue != null && b.Venue.Availability == status);
            }
            // =================================================================

            // Event Type Filter
            if (!string.IsNullOrEmpty(eventType) && int.TryParse(eventType, out int etId))
            {
                query = query.Where(b => b.Event != null && b.Event.EventTypeID == etId);
            }

            // Date Range
            if (startDateFrom.HasValue)
                query = query.Where(b => b.StartDate >= startDateFrom.Value);

            if (endDateTo.HasValue)
                query = query.Where(b => b.EndDate <= endDateTo.Value);

            var bookings = await query
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            // Populate Event Types for dropdown
            ViewBag.EventTypes = await _context.EventTypes
                .OrderBy(et => et.EventTitle)
                .ToListAsync();

            return View(bookings);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,StartDate,EndDate,EventID,VenueID")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                bool isOverlapping = await _context.Bookings
                    .AnyAsync(b => b.VenueID == booking.VenueID &&
                                  b.StartDate < booking.EndDate &&
                                  b.EndDate > booking.StartDate);
                if (isOverlapping)
                {
                    ModelState.AddModelError("", "This venue is already booked for the selected time period. Please choose a different time or venue.");
                    ViewData["EventID"] = new SelectList(_context.Events, "EventID", "EventName", booking.EventID);
                    ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "VenueName", booking.VenueID);
                    return View(booking);
                }

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
                                  b.BookingID != booking.BookingID &&
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