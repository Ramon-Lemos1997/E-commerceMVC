using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infra.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Produtos> Produtos { get; set; }
        public DbSet<ShoppingCart> ShoppingCartUser { get; set; }
        public DbSet<FavoriteProducts> FavoriteProductsUser { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .ToTable("AspNetUsers"); 

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.FirstName)
                .HasMaxLength(255);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Surname)
                .HasMaxLength(255);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Gender)
                .HasMaxLength(50);
          
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.BirthDate);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.ResetPassword);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Street) 
                .HasMaxLength(100);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Neighborhood) 
                .HasMaxLength(100);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.ZipCode) 
                .HasMaxLength(8);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.City)
                .HasMaxLength(50);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.State)
                .HasMaxLength(30);

            modelBuilder.Entity<ApplicationUser>()
               .Property(u => u.CreationDate)
               .HasMaxLength(30);

            
        }

        //public DbSet<AlunoModel> Alunos { get; set; }
        //public DbSet<ProdutosModel> Produtos { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Entity<AlunoModel>().HasData(
        //      new AlunoModel
        //      {
        //          AlunoId = 1,
        //          Nome = "Ramon",
        //          Email = "johnnyramon2011@gmail.com",
        //          Idade = 26,
        //          Curso = "Computação"
        //      });

        //}



    }
}
