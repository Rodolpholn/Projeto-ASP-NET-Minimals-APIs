using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projeto_ASP_NET_Minimals_APIs.Dominio.ModelViews
{
    public class AdmLogadoModelView
    {

         public string Email { get; set; } = default!;

        public string Perfil { get; set; } = default!;  
        
        public string Token { get; set; } = default!;
    }
}