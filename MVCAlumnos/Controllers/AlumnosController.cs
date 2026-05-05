using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCAlumnos.Controllers;
using MVCAlumnos.Data;
using MVCAlumnos.Models;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MVCAlumnos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AlumnosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AlumnosController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index(string searchString, int? carreraId, int? semestre, int page = 1)
        {
            int pageSize = 3; // Número de registros por página
            var alumnos = _context.Alumnos.Include(a => a.Carrera).AsQueryable();

            // Búsqueda por nombre o correo
            if (!string.IsNullOrEmpty(searchString))
            {
                alumnos = alumnos.Where(a => a.Nombre.Contains(searchString) ||
                                              a.Correo.Contains(searchString));
            }

            // Filtro por carrera
            if (carreraId.HasValue && carreraId > 0)
            {
                alumnos = alumnos.Where(a => a.CarreraId == carreraId);
            }

            // Filtro por semestre
            if (semestre.HasValue && semestre > 0)
            {
                alumnos = alumnos.Where(a => a.Semestre == semestre);
            }

            // Paginación
            var totalRegistros = await alumnos.CountAsync();
            var items = await alumnos.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.CarreraId = new SelectList(_context.Carreras, "Id", "Nombre", carreraId);
            ViewBag.SemestreFiltro = semestre;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRegistros / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CarreraFiltro = carreraId;

            return View(items);
        }

        // GET : Alumnos/Create
        public IActionResult Create()
        {
            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre");
            return View();
        }

        // POST : Alumnos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,CarreraId,Semestre,Correo,Telefono,FechaNacimiento,FotoUrl")] Alumnos alumnos, IFormFile? foto)
        {
            if (ModelState.IsValid)
            {
                // Guardar foto
                if (foto != null && foto.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/alumnos", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }
                    alumnos.FotoUrl = "/images/alumnos/" + fileName;
                }

                // Guardar alumno
                _context.Alumnos.Add(alumnos);
                await _context.SaveChangesAsync();

                // Crear usuario Identity con el mismo correo
                var user = new IdentityUser
                {
                    UserName = alumnos.Correo,
                    Email = alumnos.Correo,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "Alumno123!");

                if (result.Succeeded)
                {
                    // Asignar rol Alumno
                    await _userManager.AddToRoleAsync(user, "Alumno");
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre", alumnos.CarreraId);
            return View(alumnos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var alumno = await _context.Alumnos
                .Include(a => a.Carrera)  // ← Agrega esta línea
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null)
            {
                return NotFound();
            }
            return View(alumno);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null) return NotFound();

            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre", alumno.CarreraId);
            return View(alumno);
        }

        // POST : Alumnos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,CarreraId,Semestre,Correo,Telefono,FechaNacimiento,FotoUrl")] Alumnos alumnos, IFormFile? foto)
        {
            if (id != alumnos.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el alumno actual de la BD para conservar la foto si no se sube una nueva
                    var alumnoExistente = await _context.Alumnos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

                    if (foto != null && foto.Length > 0)
                    {
                        // Eliminar foto anterior si existe
                        if (!string.IsNullOrEmpty(alumnoExistente?.FotoUrl))
                        {
                            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", alumnoExistente.FotoUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Guardar nueva foto
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/alumnos", fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await foto.CopyToAsync(stream);
                        }

                        alumnos.FotoUrl = "/images/alumnos/" + fileName;
                    }
                    else
                    {
                        // Si no se subió nueva foto, conservar la existente
                        alumnos.FotoUrl = alumnoExistente?.FotoUrl;
                    }

                    _context.Update(alumnos);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Alumnos.Any(a => a.Id == alumnos.Id))
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

            ViewData["CarreraId"] = new SelectList(_context.Carreras, "Id", "Nombre", alumnos.CarreraId);
            return View(alumnos);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumno = await _context.Alumnos
                .Include(a => a.Carrera)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null)
            {
                return NotFound();
            }

            return View(alumno);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno != null)
            {
                _context.Alumnos.Remove(alumno);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
