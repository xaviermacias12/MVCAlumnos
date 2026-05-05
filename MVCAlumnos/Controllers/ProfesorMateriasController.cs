using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCAlumnos.Data;
using MVCAlumnos.Models;
using Microsoft.AspNetCore.Authorization;

namespace MVCAlumnos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProfesorMateriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfesorMateriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProfesorMaterias
        public async Task<IActionResult> Index()
        {
            var items = _context.ProfesorMaterias
                .Include(pm => pm.Profesor)
                .Include(pm => pm.Materia)
                .ThenInclude(m => m.Carrera);
            return View(await items.ToListAsync());
        }

        // GET: ProfesorMaterias/Create
        public IActionResult Create()
        {
            ViewData["MateriaId"] = new SelectList(_context.Materias, "Id", "Nombre");
            ViewData["ProfesorId"] = new SelectList(_context.Profesores, "Id", "Nombre");
            return View();
        }

        // POST: ProfesorMaterias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProfesorId,MateriaId")] ProfesorMateria profesorMateria)
        {
            // Verificar si ya existe
            var existe = await _context.ProfesorMaterias
                .AnyAsync(pm => pm.ProfesorId == profesorMateria.ProfesorId &&
                                pm.MateriaId == profesorMateria.MateriaId);

            if (existe)
            {
                ModelState.AddModelError("", "Esta asignación ya existe");
            }

            if (ModelState.IsValid)
            {
                _context.Add(profesorMateria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProfesorId"] = new SelectList(_context.Profesores, "Id", "Nombre", profesorMateria.ProfesorId);
            ViewData["MateriaId"] = new SelectList(_context.Materias, "Id", "Nombre", profesorMateria.MateriaId);
            return View(profesorMateria);
        }

        // GET: ProfesorMaterias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.ProfesorMaterias
                .Include(pm => pm.Profesor)
                .Include(pm => pm.Materia)
                .FirstOrDefaultAsync(pm => pm.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // POST: ProfesorMaterias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.ProfesorMaterias.FindAsync(id);
            if (item != null)
            {
                _context.ProfesorMaterias.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProfesorMateriaExists(int id)
        {
            return _context.ProfesorMaterias.Any(e => e.Id == id);
        }
    }
}
