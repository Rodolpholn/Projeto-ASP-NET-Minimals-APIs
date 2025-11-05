using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Interfaces;
using Projeto_ASP_NET_Minimals_APIs.Infraestrutura.Db;
using Projeto_ASP_NET_Minimals_APIs.Dominio.DTOs;

namespace Projeto_ASP_NET_Minimals_APIs.Dominio.Servicos
{
    public class AdministradorServico : iAdministradorServico
    {
        private readonly DbContexto _contexto;
        public AdministradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public Administrador Adicionar(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
           _contexto.SaveChanges();

            return administrador;
        }

        public Administrador? BuscaPorId(int id)
        {
            return _contexto.Administradores.Where(v => v.Id == id).FirstOrDefault();
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();

            return adm;
            
        }

        public List<Administrador> Todos(int? pagina)
        {
            var query = _contexto.Administradores.AsQueryable();

        int intesPorPagina = 10;

            if (pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * intesPorPagina).Take(intesPorPagina);
            }
        
        return query.ToList();
        }
    }
}