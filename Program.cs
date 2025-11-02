using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projeto_ASP_NET_Minimals_APIs.Dominio.DTOs;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Interfaces;
using Projeto_ASP_NET_Minimals_APIs.Dominio.ModelViews;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Servicos;
using Projeto_ASP_NET_Minimals_APIs.Infraestrutura.Db;

#region BUILDER AND SERVICES
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<iAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<iVeiculosServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion

#region HOME COM RECORD STRUCT

// app.MapGet("/", () => Results.Json(new Home()));
app.MapGet("/", (HttpRequest request) =>
{
    var baseUrl = $"{request.Scheme}://{request.Host}";

    // Cria um novo record struct passando os valores no construtor
    var homeView = new Home(
        Mensagem: "Bem vindo a API de Veiculos - Minimal API",
        Doc: $"{baseUrl}/swagger" // URL COMPLETA
    );

    return Results.Json(homeView);
}).WithName("Home");
#endregion

#region ADMINISTRADORES
app.MapPost("/administradores/login", ([FromBody]LoginDTO loginDTO, iAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
}).WithTags("Administradores");
#endregion

#region VEICULOS
app.MapPost("/veiculos", ([FromBody] VeiculoDTO VeiculoDTO, iVeiculosServico veiculoServico) =>
{
    var veiculo = new Veiculo
    {
        Nome = VeiculoDTO.Nome,
        Marca = VeiculoDTO.Marca,
        Ano = VeiculoDTO.Ano
    };
    veiculoServico.Adicionar(veiculo);
    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
}).WithTags("Enviar Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, iVeiculosServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).WithTags("buscar Veiculos");

app.MapGet("/veiculos{id}", ([FromRoute] int id, iVeiculosServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);

    if(veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).WithTags("Buscar Veiculo por Id");

app.MapPut("/veiculos{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, iVeiculosServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);

}).WithTags("Atualizar Veiculo");
app.MapDelete("/veiculos{id}", ([FromRoute] int id,  iVeiculosServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculoServico.remover(veiculo);
return Results.Ok(veiculo);
}).WithTags("Deletar Veiculo");
#endregion

#region APP
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion
public class LoginDTO
{
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = default!;
}




