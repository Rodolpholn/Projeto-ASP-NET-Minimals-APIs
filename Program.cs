using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Projeto_ASP_NET_Minimals_APIs.Dominio.DTOs;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Enuns;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Interfaces;
using Projeto_ASP_NET_Minimals_APIs.Dominio.ModelViews;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Servicos;
using Projeto_ASP_NET_Minimals_APIs.Infraestrutura.Db;

#region BUILDER AND SERVICES
// Criação do builder
var builder = WebApplication.CreateBuilder(args);
// Configuração do JWT
var key = builder.Configuration.GetSection("Jwt").ToString();
// Caso a chave não esteja no appsettings.json, usar uma chave padrão
if(string.IsNullOrEmpty(key)) key = "1234567890123456"; // chave padrão caso não esteja no appsettings
// Configuração do serviço de autenticação
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
    };
});



builder.Services.AddAuthorization();

// Adicionando os serviços ao container de injeção de dependência
builder.Services.AddScoped<iAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<iVeiculosServico, VeiculoServico>();
// Adicionando os serviços do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
option.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
{
    Name = "Authorization",
    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    Description = "Insira o toke JWT Aqui"
});
option.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
{
    {
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }
});
});


// Configuração do DbContext com MySQL
builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});
// Construção do app
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
}).AllowAnonymous().WithName("Home");
#endregion

#region ADMINISTRADORES
string GerarTokenJwt(Administrador administrador)
{
    // if(!string.IsNullOrEmpty(key)) return string.Empty;
      var SymmetricSecurityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
    var Credentials = new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);


    var claims = new List<Claim>()
    {
        new Claim(JwtRegisteredClaimNames.Sub, administrador.Email),
        new Claim("perfil", administrador.Perfil)
    };
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddHours(2),
        signingCredentials: Credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}


app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) =>
{
    var adm = administradorServico.Login(loginDTO);
    if (adm != null)
    {
        string token = GerarTokenJwt(adm);
        return Results.Ok(new AdmLogadoModelView
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administradores");
// Buscar Administradores
app.MapGet("/administradores", ([FromQuery] int? pagina, iAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina);
    foreach (var adm in administradores)
    {
        var perfilEnum = (Perfil)Enum.Parse(typeof(Perfil), adm.Perfil);
        adms.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = perfilEnum.ToString()
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization().WithTags("Administradores");

// Buscar Veiculo por Id
app.MapGet("/administradores{id}", ([FromRoute] int id, iAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscaPorId(id);

    if(administrador == null) return Results.NotFound();
    return Results.Ok(new AdministradorModelView
        {
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
        });
}).RequireAuthorization().WithTags("Administradores");

// Criar Administrador
app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, iAdministradorServico administradorServico) =>
{
    // Validar o DTO
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(administradorDTO.Email))
        validacao.Mensagens.Add("O email do administrador é obrigatório.");
    if (string.IsNullOrEmpty(administradorDTO.Senha))
        validacao.Mensagens.Add("A senha do administrador é obrigatória.");
    if (administradorDTO.Perfil == null)
        validacao.Mensagens.Add("O perfil do administrador é obrigatório.");

    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);
    // Adicionar o Administrador
    var administrador = new Administrador
    {
        
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
        
    };

    administradorServico.Adicionar(administrador);
    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
        {
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil.ToString()
        });
    
}).RequireAuthorization().WithTags("Administradores");
#endregion

#region VEICULOS
// Função de validação do DTO
ErrosDeValidacao validaDTO(VeiculoDTO VeiculoDTO)
{
    var validacao = new ErrosDeValidacao  {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(VeiculoDTO.Nome))
        validacao.Mensagens.Add("O nome do veiculo é obrigatório.");

    if (string.IsNullOrEmpty(VeiculoDTO.Marca))
        validacao.Mensagens.Add("A marca do veiculo é obrigatória.");

    if (VeiculoDTO.Ano < 1886 || VeiculoDTO.Ano > DateTime.Now.Year + 1)
        validacao.Mensagens.Add("O ano do veiculo é inválido.");

      return validacao;
}
// CRUD de Veiculos
app.MapPost("/veiculos", ([FromBody] VeiculoDTO VeiculoDTO, iVeiculosServico veiculoServico) =>
{
    // Validar o DTO
    var validacao = validaDTO(VeiculoDTO);
    if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);
    // Adicionar o veiculo
    var veiculo = new Veiculo
    {
        Nome = VeiculoDTO.Nome,
        Marca = VeiculoDTO.Marca,
        Ano = VeiculoDTO.Ano
    };
    veiculoServico.Adicionar(veiculo);
    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
}).RequireAuthorization().WithTags("Veiculos");
// Buscar Veiculos
app.MapGet("/veiculos", ([FromQuery] int? pagina, iVeiculosServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veiculos");
// Buscar Veiculo por Id
app.MapGet("/veiculos{id}", ([FromRoute] int id, iVeiculosServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);

    if(veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veiculos");
// Atualizar Veiculo
app.MapPut("/veiculos{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, iVeiculosServico veiculoServico) =>
{

    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();
    // Validar o DTO
    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);

}).RequireAuthorization().WithTags("Veiculos");
// Deletar Veiculo
app.MapDelete("/veiculos{id}", ([FromRoute] int id,  iVeiculosServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculoServico.remover(veiculo);
return Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veiculos");
#endregion

#region APP
// Configurando o Swagger
app.UseSwagger();
app.UseSwaggerUI();
// Ativando o uso de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion
public class LoginDTO
{
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = default!;
}




