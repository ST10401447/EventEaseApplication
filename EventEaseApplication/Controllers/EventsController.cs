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
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var allEvents = await _context.Events
                .Include(e => e.EventType)
                .ToListAsync();

            // If search bar is empty → show all events
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return View(allEvents.OrderByDescending(e => e.EventDate).ToList());
            }

            // Manual filtering using loop
            List<Event> filteredEvents = new List<Event>();
            string searchLower = searchString.Trim().ToLower();

            foreach (var item in allEvents)
            {
                bool isMatch = false;

                // Search by Event Name
                if (item.EventName != null && item.EventName.ToLower().Contains(searchLower))
                {
                    isMatch = true;
                }
                // Search by Description
                else if (item.Description != null && item.Description.ToLower().Contains(searchLower))
                {
                    isMatch = true;
                }
                // Search by Event Type
                else if (item.EventType?.EventTitle != null &&
                         item.EventType.EventTitle.ToLower().Contains(searchLower))
                {
                    isMatch = true;
                }

                if (isMatch)
                {
                    filteredEvents.Add(item);
                }
            }

            return View(filteredEvents.OrderByDescending(e => e.EventDate).ToList());
        }


        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["EventTypeID"] = new SelectList(_context.EventTypes, "EventTypeID", "EventTitle");
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventID,EventName,Description,EventDate,EventTypeID")] Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventTypeID"] = new SelectList(_context.EventTypes, "EventTypeID", "EventTitle", @event.EventTypeID);
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewData["EventTypeID"] = new SelectList(_context.EventTypes, "EventTypeID", "EventTitle", @event.EventTypeID);
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventID,EventName,Description,EventDate,EventTypeID")] Event @event)
        {
            if (id != @event.EventID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventID))
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
            ViewData["EventTypeID"] = new SelectList(_context.EventTypes, "EventTypeID", "EventTitle", @event.EventTypeID);
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventID == id);

            if (@event == null)
            {
                return NotFound();
            }

            // Prevent deletion of upcoming events
            if (@event.EventDate > DateTime.Now)
            {
                TempData["ErrorMessage"] = "You cannot delete an upcoming event. Only past events can be deleted.";
                return RedirectToAction(nameof(Index));
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);

            if (@event != null)
            {
                // Double protection to Prevent deletion of future events
                if (@event.EventDate > DateTime.Now)
                {
                    TempData["ErrorMessage"] = "You cannot delete an upcoming event.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventID == id);
        }
    }
}