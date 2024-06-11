using DiyetisyenDanisanWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DiyetisyenDanisanWebApp.Services
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Mesaj> Mesajlar { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-5GM9TLM\\SQLEXPRESS;Database=DiyetisyenDanisanDB;Trusted_Connection=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

}
