using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVCAlumnos.Models;

namespace MVCAlumnos.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Alumnos> Alumnos { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<AlumnoMateria> AlumnoMaterias { get; set; }
        public DbSet<Carrera> Carreras { get; set; }
        public DbSet<Profesor> Profesores { get; set; }
        public DbSet<ProfesorMateria> ProfesorMaterias { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProfesorMateria>()
                .HasIndex(pm => new { pm.ProfesorId, pm.MateriaId })
                .IsUnique();

            modelBuilder.Entity<AlumnoMateria>()
                .HasKey(am => new { am.AlumnoId, am.MateriaId });

            modelBuilder.Entity<AlumnoMateria>()
                .HasOne(am => am.Alumno)
                .WithMany(a => a.AlumnoMaterias)
                .HasForeignKey(am => am.AlumnoId);

            modelBuilder.Entity<AlumnoMateria>()
                .HasOne(am => am.Materia)
                .WithMany(m => m.AlumnoMaterias)
                .HasForeignKey(am => am.MateriaId);

            modelBuilder.Entity<Profesor>()
                .HasOne(p => p.Carrera)
                .WithMany(c => c.Profesores)
                .HasForeignKey(p => p.CarreraId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Materia>()
                .HasOne(m => m.Carrera)
                .WithMany(c => c.Materias)
                .HasForeignKey(m => m.CarreraId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Carrera>()
                .HasIndex(c => c.Codigo)
                .IsUnique();

            modelBuilder.Entity<Profesor>()
                .HasIndex(p => p.Correo)
                .IsUnique();

            modelBuilder.Entity<Alumnos>()
                .HasIndex(a => a.Correo)
                .IsUnique();
        }
    }
}