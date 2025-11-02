using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projeto_ASP_NET_Minimals_APIs.Dominio.ModelViews
{
    // public struct Home
    // {
    //     public string Mensagem { get => "Bem vindo a API de Veiculos - Minimal API "; } 
    //     public string Doc { get => "/swagger"; } 
    // }
    public readonly record struct Home(string Mensagem, string Doc);
}