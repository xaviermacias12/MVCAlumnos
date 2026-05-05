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
    [Authorize(Roles = "Admin,Profesor")]
    public class AlumnoMateriasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AlumnoMateriasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AlumnoMaterias
        public async Task<IActionResult> Index(int? materiaId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var userEmail = user?.Email;

            IQueryable<AlumnoMateria> query = _context.AlumnoMaterias
                .Include(a => a.Alumno)
                .Include(a => a.Materia)
                .ThenInclude(m => m.Carrera);

            // Si es Profesor, filtrar por materias que imparte
            if (User.IsInRole("Profesor"))
            {
                var profesor = await _context.Profesores
                    .FirstOrDefaultAsync(p => p.Correo == userEmail);

                if (profesor != null)
                {
                    var materiasDelProfesor = await _context.ProfesorMaterias
                        .Where(pm => pm.ProfesorId == profesor.Id)
                        .Select(pm => pm.MateriaId)
                        .ToListAsync();

                    query = query.Where(am => materiasDelProfesor.Contains(am.MateriaId));

                    // Filtrar por materia específica si se pasa el parámetro
                    if (materiaId.HasValue && materiasDelProfesor.Contains(materiaId.Value))
                    {
                        query = query.Where(am => am.MateriaId == materiaId);
                    }
                }
            }
            // Si es Alumno, filtrar por el mismo alumno
            else if (User.IsInRole("Alumno"))
            {
                var alumno = await _context.Alumnos
                    .FirstOrDefaultAsync(a => a.Correo == userEmail);

                if (alumno != null)
                {
                    query = query.Where(am => am.AlumnoId == alumno.Id);
                }
            }

            return View(await query.ToListAsync());
        }

        // GET: AlumnoMaterias/Details/5/3
        public async Task<IActionResult> Details(int alumnoId, int materiaId)
        {
            var alumnoMateria = await _context.AlumnoMaterias
                .Include(a => a.Alumno)
                .Include(a => a.Materia)
                .ThenInclude(m => m.Carrera)
                .FirstOrDefaultAsync(m => m.AlumnoId == alumnoId && m.MateriaId == materiaId);

            if (alumnoMateria == null)
            {
                return NotFound();
            }

            return View(alumnoMateria);
        }

        // GET: AlumnoMaterias/Create
        public IActionResult Create(int? materiaId = null)
        {
            // Si es Profesor, filtrar alumnos por su carrera
            if (User.IsInRole("Profesor"))
            {
                var user = _userManager.GetUserAsync(User).Result;
                var profesor = _context.Profesores.FirstOrDefault(p => p.Correo == user.Email);

                if (profesor != null)
                {
                    // Obtener las materias del profesor
                    var materiasDelProfesor = _context.ProfesorMaterias
                        .Where(pm => pm.ProfesorId == profesor.Id)
                        .Select(pm => pm.MateriaId)
                        .ToList();

                    var materiasFiltradas = _context.Materias
                        .Where(m => materiasDelProfesor.Contains(m.Id))
                        .ToList();

                    ViewData["MateriaId"] = new SelectList(materiasFiltradas, "Id", "Nombre", materiaId);

                    // Si hay una materia seleccionada, obtener su CarreraId
                    int? carreraId = null;
                    if (materiaId.HasValue)
                    {
                        var materia = _context.Materias.Find(materiaId.Value);
                        carreraId = materia?.CarreraId;
                    }
                    else if (materiasFiltradas.Any())
                    {
                        carreraId = materiasFiltradas.First().CarreraId;
                    }

                    // Filtrar alumnos por la misma carrera
                    var alumnosFiltrados = _context.Alumnos
                        .Where(a => a.CarreraId == carreraId)
                        .ToList();

                    ViewData["AlumnoId"] = new SelectList(alumnosFiltrados, "Id", "Nombre");
                }
                else
                {
                    ViewData["AlumnoId"] = new SelectList(_context.Alumnos, "Id", "Nombre");
                    ViewData["MateriaId"] = new SelectList(_context.Materias, "Id", "Nombre");
                }
            }
            else
            {
                ViewData["AlumnoId"] = new SelectList(_context.Alumnos, "Id", "Nombre");
                ViewData["MateriaId"] = new SelectList(_context.Materias, "Id", "Nombre", materiaId);
            }

            return View();
        }

        // POST: AlumnoMaterias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlumnoMateria alumnoMateria)
        {
            // Verificar que los IDs no sean cero
            if (alumnoMateria.AlumnoId == 0 || alumnoMateria.MateriaId == 0)
            {
                ViewData["AlumnoId"] = new SelectList(_context.Alumnos, "Id", "Nombre");
                ViewData["MateriaId"] = new SelectList(_context.Materias, "Id", "Nombre");
                return View(alumnoMateria);
            }

            // Si es Profesor, validar que el alumno pertenezca a la misma carrera que la materia
            if (User.IsInRole("Profesor"))
            {
                var user = await _userManager.GetUserAsync(User);
                var profesor = await _context.Profesores
                    .FirstOrDefaultAsync(p => p.Correo == user.Email);

                if (profesor != null)
                {
                    // Verificar que la materia es del profesor
                    var materiaEsDelProfesor = await _context.ProfesorMaterias
                        .AnyAsync(pm => pm.ProfesorId == profesor.Id && pm.MateriaId == alumnoMateria.MateriaId);

                    if (!materiaEsDelProfesor)
                    {
                        ModelState.AddModelError("", "No puedes asignar alumnos a materias que no impartes");
                        ViewData["AlumnoId"] = new SelectList(_context.Alumnos, "Id", "Nombre");
                        ViewData["MateriaId"] = new SelectList(_context.Materias, "Id", "Nombre");
                        return View(alumnoMateria);
                    }

                    // Obtener la carrera de la materia
                    var materia = await _context.Materias.FindAsync(alumnoMateria.MateriaId);
                    var alumno = await _context.Alumnos.FindAsync(alumnoMateria.AlumnoId);

                    // Validar que el alumno pertenezca a la misma carrera
                    if (materia.CarreraId != alumno?.CarreraId)
                    {
                        ModelState.AddModelError("", "El alumno debe pertenecer a la misma carrera que la materia");
                        ViewData["AlumnoId"] = new SelectList(_context.Alumnos, "Id", "Nombre");
                        ViewData["MateriaId"] = new SelectList(_context.Materias, "Id", "Nombre");
                        return View(alumnoMateria);
                    }
                }
            }

            // Verificar si ya existe
            var existe = await _context.AlumnoMaterias
                .AnyAsync(am => am.AlumnoId == alumnoMateria.AlumnoId && am.MateriaId == alumnoMateria.MateriaId);

            if (existe)
            {
                ModelState.AddModelError("", "Esta relación ya existe");
                ViewData["AlumnoId"] = new SelectList(_context.Alumnos, "Id", "Nombre", alumnoMateria.AlumnoId);
                ViewData["MateriaId"] = new SelectList(_context.Materias, "Id", "Nombre", alumnoMateria.MateriaId);
                return View(alumnoMateria);
            }

            // Guardar
            _context.AlumnoMaterias.Add(alumnoMateria);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: AlumnoMaterias/Delete/5/3
        public async Task<IActionResult> Delete(int alumnoId, int materiaId)
        {
            var alumnoMateria = await _context.AlumnoMaterias
                .Include(a => a.Alumno)
                .Include(a => a.Materia)
                .FirstOrDefaultAsync(m => m.AlumnoId == alumnoId && m.MateriaId == materiaId);

            if (alumnoMateria == null)
            {
                return NotFound();
            }

            return View(alumnoMateria);
        }

        // POST: AlumnoMaterias/Delete/5/3
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int alumnoId, int materiaId)
        {
            var alumnoMateria = await _context.AlumnoMaterias
                .FirstOrDefaultAsync(am => am.AlumnoId == alumnoId && am.MateriaId == materiaId);

            if (alumnoMateria != null)
            {
                _context.AlumnoMaterias.Remove(alumnoMateria);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Solo UN método AlumnoMateriaExists
        private bool AlumnoMateriaExists(int alumnoId, int materiaId)
        {
            return _context.AlumnoMaterias.Any(e => e.AlumnoId == alumnoId && e.MateriaId == materiaId);
        }
    }
}
