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
    public class VenueTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenueTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VenueTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.VenueTypes.ToListAsync());
        }

        // GET: VenueTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venueType = await _context.VenueTypes
                .FirstOrDefaultAsync(m => m.VenueTypeID == id);
            if (venueType == null)
            {
                return NotFound();
            }

            return View(venueType);
        }

        // GET: VenueTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VenueTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueTypeID,VenueTitle")] VenueType venueType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(venueType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venueType);
        }

        // GET: VenueTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venueType = await _context.VenueTypes.FindAsync(id);
            if (venueType == null)
            {
                return NotFound();
            }
            return View(venueType);
        }

        // POST: VenueTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueTypeID,VenueTitle")] VenueType venueType)
        {
            if (id != venueType.VenueTypeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venueType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueTypeExists(venueType.VenueTypeID))
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
            return View(venueType);
        }

        // GET: VenueTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venueType = await _context.VenueTypes
                .FirstOrDefaultAsync(m => m.VenueTypeID == id);
            if (venueType == null)
            {
                return NotFound();
            }

            return View(venueType);
        }

        // POST: VenueTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venueType = await _context.VenueTypes.FindAsync(id);
            if (venueType != null)
            {
                _context.VenueTypes.Remove(venueType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueTypeExists(int id)
        {
            return _context.VenueTypes.Any(e => e.VenueTypeID == id);
        }
    }
}
