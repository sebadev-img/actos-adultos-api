using ActosAdultosAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ActosAdultosAPI.Data
{
    public class ApplicationDbContext: IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<Ciudad> Ciudades { get; set; }
        public DbSet<Acto> Actos { get; set; }
        public DbSet<Institucion> Instituciones { get; set; }
        public DbSet<Postulante> Postulantes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Orientacion> Orientaciones { get; set; }
        public DbSet<Cargo> Cargos { get; set; }
        public DbSet<CargoHorario> CargoHorarios { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // REFRESH TOKEN
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TokenHash)
                    .HasMaxLength(64)
                    .IsRequired();

                entity.HasIndex(x => x.TokenHash)
                    .IsUnique();

                entity.HasIndex(x => x.UsuarioId);

                entity.HasOne(x => x.Usuario)
                    .WithMany()
                    .HasForeignKey(x => x.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Institucion - AppUser (1:1 opcional)
            builder.Entity<Institucion>()
                .HasOne(i => i.Usuario)
                .WithOne(u => u.Institucion)
                .HasForeignKey<Institucion>(i => i.UsuarioId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Institucion - Ciudad (N:1)
            builder.Entity<Institucion>()
                .HasOne(i => i.Ciudad)
                .WithMany()
                .HasForeignKey(i => i.CiudadId)
                .OnDelete(DeleteBehavior.Restrict);

            // APPUSER - POSTULANTE (1:1 OBLIGATORIA DESDE POSTULANTE)
            builder.Entity<Postulante>()
                .HasOne(p => p.Usuario)
                .WithOne(u => u.Postulante)
                .HasForeignKey<Postulante>(p => p.UsuarioId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // POSTULANTE - CIUDAD (N:1)
            builder.Entity<Postulante>()
                .HasOne(p => p.Ciudad)
                .WithMany()
                .HasForeignKey(p => p.CiudadId)
                .OnDelete(DeleteBehavior.Restrict);

            // POSTULANTE - DNI UNICO
            builder.Entity<Postulante>()
                .HasIndex(p => p.Dni)
                .IsUnique();

            // ACTO - CARGO (1:N)
            builder.Entity<Cargo>()
                .HasOne(c => c.Acto)
                .WithMany(a => a.Cargos)
                .HasForeignKey(c => c.ActoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // INSTITUCION - CARGO (1:N)
            builder.Entity<Cargo>()
                .HasOne(c => c.Institucion)
                .WithMany(i => i.Cargos)
                .HasForeignKey(c => c.InstitucionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // ORIENTACION - CARGO (1:N)
            builder.Entity<Cargo>()
                .HasOne(c => c.Orientacion)
                .WithMany(o => o.Cargos)
                .HasForeignKey(c => c.OrientacionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // CARGO - CARGOHORARIO (1:N)
            builder.Entity<CargoHorario>()
                .HasOne(h => h.Cargo)
                .WithMany(c => c.Horarios)
                .HasForeignKey(h => h.CargoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // INSCRIPCION
            // CARGO - INSCRIPCIONES (1:N)
            builder.Entity<Inscripcion>()
                .HasOne(i => i.Cargo)
                .WithMany(c => c.Inscripciones)
                .HasForeignKey(i => i.CargoId)
                .OnDelete(DeleteBehavior.Restrict);

            // POSTULANTE - INSCRIPCIONES (1:N)
            builder.Entity<Inscripcion>()
                .HasOne(i => i.Postulante)
                .WithMany(p => p.Inscripciones)
                .HasForeignKey(i => i.PostulanteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Inscripcion>()
                .HasIndex(i => new
                {
                    i.CargoId,
                    i.PostulanteId
                })
                .IsUnique()
                .HasFilter("[Eliminado] = 0");

            builder.Entity<Inscripcion>()
                .Property(i => i.Caracter)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Entity<Inscripcion>()
                .Property(i => i.PuntajeJunta)
                .HasPrecision(18, 4);

            builder.Entity<Inscripcion>()
                .HasIndex(i => i.CargoId)
                .IsUnique()
                .HasFilter(
                    "[Eliminado] = 0 AND [EstadoInscripcion] = 1");
        }

    }
}
