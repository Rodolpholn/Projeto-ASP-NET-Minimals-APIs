using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Servicos;
using Projeto_ASP_NET_Minimals_APIs.Infraestrutura.Db;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;
using System.Reflection;

namespace Test.Domain.Servicos
{
    public class AdministradorServicoTest
    {
        //Cria o contexto de teste com base nas configurações do appsettings.json
        private DbContexto CriarContextoDeTeste()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "","..","..","..","..","Test"));
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(path ?? Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();
            

            return new DbContexto(configuration);
        }
        [TestMethod]
        public void TestandoSalvarAdministrador()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE  Administradores");

            var adm = new Administrador();
            adm.Email = " administrador@teste.com";
            adm.Senha = "123456";
            adm.Perfil = "Adm";
            var administradorServico = new AdministradorServico(context);

            // Act

            administradorServico.Adicionar(adm);

            // Assert
            Assert.AreEqual(1, administradorServico.Todos(1).Count());


        }
    [TestMethod]
        public void TestandoBuscaPorId()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE  Administradores");

            var adm = new Administrador();
            adm.Email = " administrador@teste.com";
            adm.Senha = "123456";
            adm.Perfil = "Adm";
            var administradorServico = new AdministradorServico(context);

            // Act

            administradorServico.Adicionar(adm);
            var admDoBanco = administradorServico.BuscaPorId(adm.Id);

            // Assert
            Assert.AreEqual(1, admDoBanco?.Id);


        }
    

}}