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
using Microsoft.AspNetCore.Identity;

namespace MVCAlumnos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProfesorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ProfesorsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Profesors
        public async Task<IActionResult> Index(string searchString, int? carreraId, int page = 1)
        {
            int pageSize = 3;
            var profesores = _context.Profesores.Include(p => p.Carrera).AsQueryable();

            // Búsqueda por nombre, correo o especialidad
            if (!string.IsNullOrEmpty(searchString))
            {
                profesores = profesores.Where(p => p.Nombre.Contains(searchString) ||
                                                    p.Correo.Contains(searchString) ||
                                                    p.Especialidad.Contains(searchString));
            }

            // Filtro por carrera
            if (carreraId.HasValue && carreraId > 0)
            {
                profesores = profesores.Where(p => p.CarreraId == carreraId);
            }

            // Paginación
            var totalRegistros = await profesores.CountAsync();
            var items = await profesores.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.CarreraId = new SelectList(_context.Carreras, "Id", "Nombre", carreraId);
            ViewBag.CarreraFiltro = carreraId;  // ← Agrega esta línea
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRegistros / pageSize);
            ViewBag.CurrentPage = page;

            return View(items);
        }

        // GET: Profesors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var profesor = await _context.Profesores
                .Include(p => p.Carrera)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (profesor == null) return NotFound();

            return View(profesor);
        }

        // GET: Profesors/Create
        public IActionResult Create()
        {
            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre");
            return View();
        }

        // POST: Profesors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Correo,Telefono,Especialidad,CarreraId")] Profesor profesor, IFormFile? foto)
        {
            if (ModelState.IsValid)
            {
                // Guardar foto
                if (foto != null && foto.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profesores", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }
                    profesor.FotoUrl = "/images/profesores/" + fileName;
                }

                // Guardar profesor
                _context.Add(profesor);
                await _context.SaveChangesAsync();

                // Crear usuario Identity con el mismo correo
                var user = new IdentityUser
                {
                    UserName = profesor.Correo,
                    Email = profesor.Correo,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "Profe123!");

                if (result.Succeeded)
                {
                    // Asignar rol Profesor
                    await _userManager.AddToRoleAsync(user, "Profesor");
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre", profesor.CarreraId);
            return View(profesor);
        }

        // GET: Profesors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var profesor = await _context.Profesores.FindAsync(id);
            if (profesor == null) return NotFound();

            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre", profesor.CarreraId);
            return View(profesor);
        }

        // POST: Profesors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Correo,Telefono,Especialidad,CarreraId,FotoUrl")] Profesor profesor, IFormFile? foto)
        {
            if (id != profesor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el profesor actual de la BD
                    var profesorExistente = await _context.Profesores.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

                    if (foto != null && foto.Length > 0)
                    {
                        // Eliminar foto anterior si existe
                        if (!string.IsNullOrEmpty(profesorExistente?.FotoUrl))
                        {
                            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", profesorExistente.FotoUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Guardar nueva foto
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profesores", fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await foto.CopyToAsync(stream);
                        }

                        profesor.FotoUrl = "/images/profesores/" + fileName;
                    }
                    else
                    {
                        // Conservar la foto existente
                        profesor.FotoUrl = profesorExistente?.FotoUrl;
                    }

                    _context.Update(profesor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfesorExists(profesor.Id))
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

            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre", profesor.CarreraId);
            return View(profesor);
        }

        // GET: Profesors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _context.Profesores
                .Include(p => p.Carrera)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profesor == null)
            {
                return NotFound();
            }

            return View(profesor);
        }

        // POST: Profesors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profesor = await _context.Profesores.FindAsync(id);
            if (profesor != null)
            {
                _context.Profesores.Remove(profesor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfesorExists(int id)
        {
            return _context.Profesores.Any(e => e.Id == id);
        }
    }
}
