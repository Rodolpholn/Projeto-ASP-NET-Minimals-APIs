using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;
using Projeto_ASP_NET_Minimals_APIs.Dominio.ModelViews;
using Test.Helpers;

namespace Test.Requests
{
    [TestClass]
    public class AdministradorRequestTest
    {
        [ClassInitialize]

        public static void ClassInit(TestContext testContext)
        {
            Setup.ClassInit(testContext);
        }
        [TestMethod]
        public async Task TestarGetSetPropriedades()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Email = "rodolpho@teste.com",
                Senha = "123456"
            };
            var context = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");


            // Act
            var response = await Setup.client.PostAsync("/api/administradores/login", context);


            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            var result =  await response.Content.ReadAsStringAsync();
            var administrador = JsonSerializer.Deserialize<AdmLogadoModelView>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var admLogado = administrador!;
            
            Assert.IsNotNull(admLogado?.Email ?? "");
            Assert.IsNotNull(admLogado?.Email ?? "");
            Assert.IsNotNull(admLogado?.Token ?? "");


    }
    }
}