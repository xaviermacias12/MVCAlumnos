using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCAlumnos.Data;
using MVCAlumnos.Models;

namespace MVCAlumnos.Controllers
{
    [Authorize(Roles = "Profesor")]
    public class ProfesorPanelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfesorPanelController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Mis materias que imparto
        public async Task<IActionResult> MisMaterias()
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email;

            var profesor = await _context.Profesores
                .FirstOrDefaultAsync(p => p.Correo == email);

            if (profesor == null)
            {
                return NotFound();
            }

            var materias = await _context.ProfesorMaterias
                .Include(pm => pm.Materia)
                .ThenInclude(m => m.Carrera)
                .Where(pm => pm.ProfesorId == profesor.Id)
                .Select(pm => pm.Materia)
                .ToListAsync();

            return View(materias);
        }

        // Alumnos de una materia específica
        public async Task<IActionResult> AlumnosPorMateria(int materiaId)
        {
            var alumnos = await _context.AlumnoMaterias
                .Include(am => am.Alumno)
                .Where(am => am.MateriaId == materiaId)
                .Select(am => am.Alumno)
                .ToListAsync();

            ViewBag.MateriaId = materiaId;
            return View(alumnos);
        }
    }
}