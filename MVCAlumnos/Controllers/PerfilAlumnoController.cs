using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCAlumnos.Data;
using MVCAlumnos.Models;

namespace MVCAlumnos.Controllers
{
    [Authorize(Roles = "Alumno")]
    public class PerfilAlumnoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PerfilAlumnoController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email;

            var alumno = await _context.Alumnos
                .Include(a => a.Carrera)
                .FirstOrDefaultAsync(a => a.Correo == email);

            if (alumno == null)
            {
                return NotFound("No se encontraron tus datos. Contacta al administrador.");
            }

            return View(alumno);
        }

        public async Task<IActionResult> MisMaterias()
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email;

            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a => a.Correo == email);

            if (alumno == null)
            {
                return NotFound();
            }

            var materias = await _context.AlumnoMaterias
                .Include(am => am.Materia)
                .ThenInclude(m => m.Carrera)
                .Where(am => am.AlumnoId == alumno.Id)
                .Select(am => am.Materia)
                .ToListAsync();

            var profesoresPorMateria = new Dictionary<int, string>();
            foreach (var materia in materias)
            {
                var profesorMateria = await _context.ProfesorMaterias
                    .Include(pm => pm.Profesor)
                    .FirstOrDefaultAsync(pm => pm.MateriaId == materia.Id);

                if (profesorMateria?.Profesor != null)
                {
                    profesoresPorMateria[materia.Id] = profesorMateria.Profesor.Nombre;
                }
                else
                {
                    profesoresPorMateria[materia.Id] = "No asignado";
                }
            }

            ViewBag.ProfesoresPorMateria = profesoresPorMateria;
            return View(materias);
        }
    }
}