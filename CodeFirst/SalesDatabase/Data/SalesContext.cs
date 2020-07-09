using Microsoft.EntityFrameworkCore;
using P03_SalesDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P03_SalesDatabase.Data
{
    public class SalesContext : DbContext
    {
        public SalesContext() { }

        public SalesContext(DbContextOptions options)
            : base(options) { }


        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Store> Stores { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.;Database=Sales;Integrated Security = true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity
                    .Property(x => x.Name)
                    .HasMaxLength(50)
                    .IsUnicode(true);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity
                    .Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity
                    .Property(x => x.Email)
                    .HasMaxLength(80)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity
                    .Property(x => x.Name)
                    .HasMaxLength(80)
                    .IsUnicode(true);
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity
                    .HasOne(x => x.Customer)
                    .WithMany(c => c.Sales)
                    .HasForeignKey(x => x.CustomerId);

                entity
                    .HasOne(x => x.Product)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(x => x.SaleId);

                entity
                    .HasOne(x => x.Store)
                    .WithMany(s => s.Sales)
                    .HasForeignKey(x => x.StoreId);
            });
        }
    }
}
