using Microsoft.EntityFrameworkCore;
using P01_HospitalDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P01_HospitalDatabase.Data 
{
    public class HospitalContext : DbContext
    {
        public HospitalContext() { }

        public HospitalContext(DbContextOptions options)
            : base(options) { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visitation> Visitations { get; set; }
        public DbSet<Medicament> Medicaments { get; set; }
        public DbSet<Diagnose> Diagnoses { get; set; }
        public DbSet<PatientMedicament> Prescriptions { get; set; }
        public DbSet<Doctor> Doctors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.;Database=Hospital;Integrated Security = true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(x => x.PatientId);

                entity
                    .Property(x => x.FirstName)
                    .HasMaxLength(50)
                    .IsUnicode(true);

                entity
                    .Property(x => x.LastName)
                    .HasMaxLength(50)
                    .IsUnicode(true);

                entity
                    .Property(x => x.Address)
                    .HasMaxLength(250)
                    .IsUnicode(true);

                entity
                    .Property(x => x.Email)
                    .HasMaxLength(80)
                    .IsUnicode(false);


            });

            modelBuilder.Entity<Visitation>(entity =>
            {
                entity.HasKey(x => x.VisitationId);

                entity
                    .Property(x => x.Comments)
                    .HasMaxLength(250)
                    .IsUnicode(true);

                entity
                    .HasOne(x => x.Patient)
                    .WithMany(p => p.Visitations)
                    .HasForeignKey(x => x.PatientId);

                entity
                    .HasOne(x => x.Doctor)
                    .WithMany(d => d.Visitations)
                    .HasForeignKey(x => x.DoctorId);
            });

            modelBuilder.Entity<Diagnose>(entity =>
            {
                entity.HasKey(x => x.DiagnoseId);

                entity
                    .Property(x => x.Name)
                    .HasMaxLength(50)
                    .IsUnicode(true);

                entity
                    .Property(x => x.Comments)
                    .HasMaxLength(250)
                    .IsUnicode(true);

                entity
                    .HasOne(x => x.Patient)
                    .WithMany(p => p.Diagnoses)
                    .HasForeignKey(x => x.PatientId);
            });

            modelBuilder.Entity<Medicament>(entity =>
            {
                entity.HasKey(x => x.MedicamentId);

                entity
                    .Property(x => x.Name)
                    .HasMaxLength(50)
                    .IsUnicode(true);
            });

            modelBuilder.Entity<PatientMedicament>(entity =>
            {
                entity.HasKey(x => new { x.MedicamentId, x.PatientId });

                entity
                    .HasOne(x => x.Patient)
                    .WithMany(p => p.Prescriptions)
                    .HasForeignKey(x => x.PatientId);

                entity
                    .HasOne(x => x.Medicament)
                    .WithMany(m => m.Prescriptions)
                    .HasForeignKey(x => x.MedicamentId);
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(x => x.DoctorId);

                entity
                    .Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity
                    .Property(x => x.Speciality)
                    .HasMaxLength(100)
                    .IsUnicode(true);
            });
        }
    }
}
