using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseApplication.Data;
using EventEaseApplication.Models;
using EventEaseApplication.Service;

namespace EventEaseApplication.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IBlobStorageService _blobStorageService;
        public VenuesController(ApplicationDbContext context, IBlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // GET: Venues

       
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var allVenues = await _context.Venues
                .Include(v => v.VenueType)
                .ToListAsync();

            // If search bar is empty → show all venues
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return View(allVenues.OrderBy(v => v.VenueName).ToList());
            }

            List<Venue> filteredVenues = new List<Venue>();
            string search = searchString.Trim();

            foreach (var venue in allVenues)
            {
                bool isMatch = false;

                // Search by Venue Name
                if (!string.IsNullOrEmpty(venue.VenueName) &&
                    venue.VenueName.Contains(search, StringComparison.OrdinalIgnoreCase))
                {
                    isMatch = true;
                }
                // Search by Location
                else if (!string.IsNullOrEmpty(venue.Location) &&
                         venue.Location.Contains(search, StringComparison.OrdinalIgnoreCase))
                {
                    isMatch = true;
                }
                // STRICT CAPACITY SEARCH
                else if (venue.Capacity.ToString() == search)        
                {
                    isMatch = true;
                }
               

                if (isMatch)
                {
                    filteredVenues.Add(venue);
                }
            }

            return View(filteredVenues.OrderBy(v => v.VenueName).ToList());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .Include(v => v.VenueType)
                .FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            ViewData["VenueTypeID"] = new SelectList(_context.VenueTypes, "VenueTypeID", "VenueTitle");
            return View();
        }

        // POST: Venues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueID,VenueName,Location,Capacity,ImageURL,VenueTypeID")] Venue venue, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                string imageUrl = null;

                // Handle file upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    try
                    {
                        imageUrl = await _blobStorageService.UploadFileAsync(ImageFile);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Image upload failed: {ex.Message}");
                        ViewBag.VenueTypeID = new SelectList(_context.VenueTypes, "VenueTypeID", "VenueTitle", venue.VenueTypeID);
                        return View(venue);
                    }
                }

                // Assign the URL to the model
                venue.ImageURL = imageUrl;
                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueTypeID"] = new SelectList(_context.VenueTypes, "VenueTypeID", "VenueTitle", venue.VenueTypeID);
            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            ViewData["VenueTypeID"] = new SelectList(_context.VenueTypes, "VenueTypeID", "VenueTitle", venue.VenueTypeID);
            return View(venue);
        }

        // POST: Venues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueID,VenueName,Location,Capacity,ImageURL,VenueTypeID")] Venue venue,
            IFormFile ImageFile)
        {
            if (id != venue.VenueID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string imageUrl = venue.ImageURL; 

                // Handle new image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    try
                    {
                        imageUrl = await _blobStorageService.UploadFileAsync(ImageFile);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Image upload failed: {ex.Message}");
                        ViewData["VenueTypeID"] = new SelectList(_context.VenueTypes, "VenueTypeID", "VenueTitle", venue.VenueTypeID);
                        return View(venue);
                    }
                }

                venue.ImageURL = imageUrl;

                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueID))
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

            ViewData["VenueTypeID"] = new SelectList(_context.VenueTypes, "VenueTypeID", "VenueTitle", venue.VenueTypeID);
            return View(venue);
        }


        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .Include(v => v.VenueType)
                .FirstOrDefaultAsync(m => m.VenueID == id);

            if (venue == null)
            {
                return NotFound();
            }

            // Prevent deletion if venue has active or upcoming bookings
            bool hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.VenueID == id && b.StartDate >= DateTime.Now);

            if (hasActiveBookings)
            {
                TempData["ErrorMessage"] = "You cannot delete this venue because it has active or upcoming bookings.";
                return RedirectToAction(nameof(Index));
            }

            return View(venue);

        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);

            if (venue != null)
            {
                // Double-check protection
                bool hasActiveBookings = await _context.Bookings
                    .AnyAsync(b => b.VenueID == id && b.StartDate >= DateTime.Now);

                if (hasActiveBookings)
                {
                    TempData["ErrorMessage"] = "You cannot delete this venue because it has active or upcoming bookings.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueID == id);
        }
    }
    
}
