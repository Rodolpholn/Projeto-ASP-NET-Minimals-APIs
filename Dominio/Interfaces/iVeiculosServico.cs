using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;

namespace Projeto_ASP_NET_Minimals_APIs.Dominio.Interfaces
{
    public interface iVeiculosServico
    {
        List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);
        Veiculo? BuscaPorId(int id);
        void Adicionar(Veiculo veiculo);
        void Atualizar(Veiculo veiculo);
        void remover(Veiculo veiculo);
        
    }
}