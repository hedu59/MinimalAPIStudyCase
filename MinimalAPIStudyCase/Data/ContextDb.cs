using Microsoft.EntityFrameworkCore;
using MinimalAPIStudyCase.Models;

namespace MinimalAPIStudyCase.Data
{
    public class ContextDb : DbContext
    {
        public ContextDb(DbContextOptions<ContextDb> options) : base(options) { }

        public DbSet<Toy> Toys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        { 
            modelBuilder.Entity<Toy>().HasKey(x => x.Id);
            modelBuilder.Entity<Toy>().Property(x=> x.Name).IsRequired().HasColumnType("varchar(100)");
            modelBuilder.Entity<Toy>().Property(x => x.Description).IsRequired().HasColumnType("varchar(300)");
            modelBuilder.Entity<Toy>().Property(x => x.Price).IsRequired().HasColumnType("decimal(38,2)");
            modelBuilder.Entity<Toy>().Property(x => x.TypeToy).IsRequired();
            modelBuilder.Entity<Toy>().Property(x => x.IsActive).IsRequired();
        }

    }
}
