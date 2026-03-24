using Microsoft.EntityFrameworkCore;
using SolarRent.Core.Models;
using SolarRent.Model;
using System.Reflection.Emit;

namespace SolarRent.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Equipment> Equipments => Set<Equipment>();
        public DbSet<RentalOrder> RentalOrders => Set<RentalOrder>();
        public DbSet<DefectRecord> DefectRecords => Set<DefectRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Индексы для часто используемых полей
            modelBuilder.Entity<RentalOrder>()
                .HasIndex(r => r.Status);
            modelBuilder.Entity<RentalOrder>()
                .HasIndex(r => r.StartDate);
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Phone);
            modelBuilder.Entity<Equipment>()
                .HasIndex(e => e.Status);
        }
    }
}