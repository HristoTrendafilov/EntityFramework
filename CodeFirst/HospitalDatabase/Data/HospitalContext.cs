using Microsoft.EntityFrameworkCore;
using P01_HospitalDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P01_HospitalDatabase.Data 
{
    public class HospitalContext : DbContext
    {
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
                entity
                    .Property(x => x.Comments)
                    .HasMaxLength(250)
                    .IsUnicode(true);

                entity
                    .HasOne(x => x.Patient)
                    .WithMany(p => p.Visitations)
                    .HasForeignKey(x => x.PatientId);
            });

            modelBuilder.Entity<Diagnose>(entity =>
            {
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
                entity
                    .Property(x => x.Name)
                    .HasMaxLength(50)
                    .IsUnicode(true);
            });

            modelBuilder.Entity<PatientMedicament>(entity =>
            {

                entity
                    .HasOne(x => x.Patient)
                    .WithMany(p => p.Prescriptions)
                    .HasForeignKey(x => x.PatientId);

                entity
                    .HasOne(x => x.Medicament)
                    .WithMany(m => m.Prescriptions)
                    .HasForeignKey(x => x.MedicamentId);
            });
        }
    }
}
