using System.ComponentModel.DataAnnotations;

namespace MVCAlumnos.Models
{
    public class AlumnoMateria
    {
        [Key]
        public int AlumnoId { get; set; }
        [Key]
        public int MateriaId { get; set; }
        public Alumnos Alumno { get; set; }
        public Materia Materia { get; set; }
    }
}
