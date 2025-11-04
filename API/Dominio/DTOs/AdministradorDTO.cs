using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Enuns;

namespace Projeto_ASP_NET_Minimals_APIs.Dominio.DTOs
{
    public class AdministradorDTO
    {
        public int Id { get; set;  } = default!;
        public string Email { get; set;  } = default!;
        public string Senha { get; set; } = default!;
        public Perfil? Perfil { get; set; } = default!;
    }
}