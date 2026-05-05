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
        private readonly UserManager<ApplicationUser> _userManager;

        public PerfilAlumnoController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Ver mi perfil
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

        // Editar perfil (GET)
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email;

            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a => a.Correo == email);

            if (alumno == null)
            {
                return NotFound();
            }

            return View(alumno);
        }

        // Editar perfil (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Telefono,FotoUrl")] Alumnos alumno, IFormFile? foto)
        {
            if (id != alumno.Id)
            {
                return NotFound();
            }

            // Obtener el alumno actual de la BD
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

                alumno.FotoUrl = "/images/alumnos/" + fileName;
            }
            else
            {
                // Conservar la foto existente
                alumno.FotoUrl = alumnoExistente?.FotoUrl;
            }

            // Conservar datos que no debe modificar
            alumno.Nombre = alumnoExistente.Nombre;
            alumno.CarreraId = alumnoExistente?.CarreraId ?? 0;
            alumno.Semestre = alumnoExistente?.Semestre ?? 1;
            alumno.Correo = alumnoExistente?.Correo ?? "";
            alumno.FechaNacimiento = alumnoExistente?.FechaNacimiento ?? DateTime.Now;

            _context.Update(alumno);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Perfil actualizado correctamente";
            return RedirectToAction(nameof(Index));
        }

        // Ver mis materias
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

            // Obtener profesor de cada materia
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