using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCAlumnos.Models
{
    public class Carrera
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la carrera es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre de la Carrera")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        [Display(Name = "Código de la Carrera")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La duración es obligatoria")]
        [Range(4, 12, ErrorMessage = "La duración debe estar entre 4 y 12 semestres")]
        [Display(Name = "Duración (semestres)")]
        public int DuracionSemestres { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        // Relaciones
        public List<Profesor>? Profesores { get; set; }
        public List<Materia>? Materias { get; set; }
    }
}