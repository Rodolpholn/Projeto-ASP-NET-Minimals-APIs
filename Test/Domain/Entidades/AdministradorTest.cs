using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;

namespace Test.Domain.Entidades
{
    public class AdministradorTest
    {
        [TestMethod] 
        public void TestarGetSetPropriedades()
        {
            // Arrange
            var adm = new Administrador();

            // Act
            adm.Id = 1;
            adm.Email = " administrador@teste.com";
            adm.Senha = "123456";
            adm.Perfil = "adm";

            // Assert
            Assert.AreEqual(1, adm.Id);
            Assert.AreEqual(" administrador@teste.com", adm.Email);
            Assert.AreEqual("123456", adm.Senha);
            Assert.AreEqual("adm", adm.Perfil);
    }
}}