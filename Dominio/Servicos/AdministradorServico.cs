using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Interfaces;
using Projeto_ASP_NET_Minimals_APIs.Infraestrutura.Db;

namespace Projeto_ASP_NET_Minimals_APIs.Dominio.Servicos
{
    public class AdministradorServico : iAdministradorServico
    {
        private readonly DbContexto _contexto;
        public AdministradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }
        
        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();

            return adm;
            
        }
    }
}