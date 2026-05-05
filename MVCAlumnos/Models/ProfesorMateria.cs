using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCAlumnos.Models
{
    public class ProfesorMateria
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Profesor")]
        public int ProfesorId { get; set; }

        [ForeignKey("Materia")]
        public int MateriaId { get; set; }

        public Profesor? Profesor { get; set; }
        public Materia? Materia { get; set; }
    }
}