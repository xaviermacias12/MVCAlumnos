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
    public class MateriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MateriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Materias
        public async Task<IActionResult> Index(string searchString, int? carreraId, int? creditos, int page = 1)
        {
            int pageSize = 3;
            var materias = _context.Materias.Include(m => m.Carrera).AsQueryable();

            // Búsqueda por nombre
            if (!string.IsNullOrEmpty(searchString))
            {
                materias = materias.Where(m => m.Nombre.Contains(searchString));
            }

            // Filtro por carrera
            if (carreraId.HasValue && carreraId > 0)
            {
                materias = materias.Where(m => m.CarreraId == carreraId);
            }

            // Filtro por créditos
            if (creditos.HasValue && creditos > 0)
            {
                materias = materias.Where(m => m.Creditos == creditos);
            }

            // Paginación
            var totalRegistros = await materias.CountAsync();
            var items = await materias.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.CarreraId = new SelectList(_context.Carreras, "Id", "Nombre", carreraId);
            ViewBag.CarreraFiltro = carreraId;
            ViewBag.CreditosSeleccionado = creditos;  // ← Agrega esta línea
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRegistros / pageSize);
            ViewBag.CurrentPage = page;

            return View(items);
        }

        // GET: Materias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materia = await _context.Materias
                .Include(m => m.Carrera)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (materia == null)
            {
                return NotFound();
            }

            return View(materia);
        }

        // GET: Materias/Create
        public IActionResult Create()
        {
            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre");
            return View();
        }

        // POST: Materias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Creditos,CarreraId")] Materia materia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(materia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre", materia.CarreraId);
            return View(materia);
        }

        // GET: Materias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var materia = await _context.Materias.FindAsync(id);
            if (materia == null) return NotFound();

            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre", materia.CarreraId);
            return View(materia);
        }

        // POST: Materias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Creditos,CarreraId")] Materia materia)
        {
            if (id != materia.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(materia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MateriaExists(materia.Id))
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
            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre", materia.CarreraId);
            return View(materia);
        }

        // GET: Materias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materia = await _context.Materias
                .Include(m => m.Carrera)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (materia == null)
            {
                return NotFound();
            }

            return View(materia);
        }

        // POST: Materias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var materia = await _context.Materias.FindAsync(id);
            if (materia != null)
            {
                _context.Materias.Remove(materia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MateriaExists(int id)
        {
            return _context.Materias.Any(e => e.Id == id);
        }
    }
}
