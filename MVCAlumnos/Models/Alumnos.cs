using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCAlumnos.Models
{
    public class Alumnos
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Carrera")]
        public int? CarreraId { get; set; }

        [ForeignKey("CarreraId")]
        [Display(Name = "Carrera")]
        public Carrera? Carrera { get; set; }

        [Required(ErrorMessage = "El semestre es obligatorio")]
        [Range(1, 12, ErrorMessage = "El semestre debe estar entre 1 y 12")]
        [Display(Name = "Semestre")]
        public int Semestre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Teléfono no válido")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "El teléfono debe tener entre 8 y 15 dígitos")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; } = DateTime.Now;

        // Foto (Requisito 4)
        [Display(Name = "Foto")]
        public string? FotoUrl { get; set; }

        // Relaciones
        public List<AlumnoMateria>? AlumnoMaterias { get; set; }
    }
}