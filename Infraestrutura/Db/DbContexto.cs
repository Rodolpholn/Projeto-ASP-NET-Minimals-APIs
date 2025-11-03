using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;

namespace Projeto_ASP_NET_Minimals_APIs.Infraestrutura.Db
{
    
    public class DbContexto : DbContext
    {

        private readonly IConfiguration _configuracaoAppSettings;
        public DbContexto(IConfiguration configuracaoAppSettings)
        {
              _configuracaoAppSettings = configuracaoAppSettings;
        }

        public DbSet<Administrador> Administradores { get; set; } = default!;

        public DbSet<Veiculo> Veiculos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Administrador>().HasData(
                new Administrador
                {
                    Id = 1,
                    Email = "administrador@teste.com",
                    Senha = "123456",
                    Perfil = "Adm"
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                 
                var stringDeConexao = _configuracaoAppSettings.GetConnectionString("MySql")?.ToString();
                if(!string.IsNullOrEmpty(stringDeConexao))
                {
                    optionsBuilder.UseMySql(
                        stringDeConexao,
                        ServerVersion.AutoDetect(stringDeConexao)
                    );
                }

            }
        }
    }
}