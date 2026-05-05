using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCAlumnos.Models
{
    public class Profesor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Teléfono no válido")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "El teléfono debe tener entre 8 y 15 dígitos")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especialidad es obligatoria")]
        [StringLength(100, ErrorMessage = "La especialidad no puede exceder 100 caracteres")]
        [Display(Name = "Especialidad")]
        public string Especialidad { get; set; } = string.Empty;

        [Display(Name = "Carrera")]
        public int CarreraId { get; set; }

        [ForeignKey("CarreraId")]
        [Display(Name = "Carrera")]
        public Carrera? Carrera { get; set; }

        [Display(Name = "Foto")]
        public string? FotoUrl { get; set; }
        public List<ProfesorMateria>? ProfesorMaterias { get; set; }
    }
}