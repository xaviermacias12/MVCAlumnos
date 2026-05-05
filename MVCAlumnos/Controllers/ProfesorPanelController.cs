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
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfesorPanelController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Ver mi perfil
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email;

            var profesor = await _context.Profesores
                .Include(p => p.Carrera)
                .FirstOrDefaultAsync(p => p.Correo == email);

            if (profesor == null)
            {
                return NotFound("No se encontraron tus datos. Contacta al administrador.");
            }

            return View(profesor);
        }

        // Editar perfil (GET)
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email;

            var profesor = await _context.Profesores
                .FirstOrDefaultAsync(p => p.Correo == email);

            if (profesor == null)
            {
                return NotFound();
            }

            return View(profesor);
        }

        // Editar perfil (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Telefono,Especialidad,FotoUrl")] Profesor profesor, IFormFile? foto)
        {
            if (id != profesor.Id)
            {
                return NotFound();
            }

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

            // Conservar datos que no debe modificar
            profesor.Nombre = profesorExistente.Nombre;
            profesor.Correo = profesorExistente?.Correo ?? "";
            profesor.CarreraId = profesorExistente?.CarreraId ?? 0;

            _context.Update(profesor);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Perfil actualizado correctamente";
            return RedirectToAction(nameof(Index));
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
            var materia = await _context.Materias.FindAsync(materiaId);
            ViewBag.MateriaNombre = materia?.Nombre;

            return View(alumnos);
        }
    }
}