using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MVCAlumnos.Models
{
    public class Materia
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre de la Materia")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los créditos son obligatorios")]
        [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
        [Display(Name = "Créditos")]
        public int Creditos { get; set; }

        [Display(Name = "Carrera")]
        public int? CarreraId { get; set; }

        [ForeignKey("CarreraId")]
        [Display(Name = "Carrera")]
        public Carrera? Carrera { get; set; }

        [JsonIgnore]
        public List<AlumnoMateria>? AlumnoMaterias { get; set; }
        public List<ProfesorMateria>? ProfesorMaterias { get; set; }
    }
}