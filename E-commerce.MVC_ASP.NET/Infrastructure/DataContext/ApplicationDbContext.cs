using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.MVC_ASP.NET.Infrastructure.DataContext
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
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
