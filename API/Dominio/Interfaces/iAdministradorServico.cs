using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projeto_ASP_NET_Minimals_APIs.Dominio.DTOs;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;

namespace Projeto_ASP_NET_Minimals_APIs.Dominio.Interfaces
{
    public interface iAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);

        Administrador Adicionar(Administrador administrador);
        Administrador? BuscaPorId(int pagina);

        List<Administrador> Todos(int? pagina);
    }
}