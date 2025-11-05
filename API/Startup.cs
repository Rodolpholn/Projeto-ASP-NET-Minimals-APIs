using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Projeto_ASP_NET_Minimals_APIs;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Entidades;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Enuns;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Interfaces;
using Projeto_ASP_NET_Minimals_APIs.Dominio.ModelViews;
using Projeto_ASP_NET_Minimals_APIs.Dominio.Servicos;
using Projeto_ASP_NET_Minimals_APIs.Dominio.DTOs;
using Projeto_ASP_NET_Minimals_APIs.Infraestrutura.Db;


    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            key = configuration?.GetSection("Jwt")?.ToString() ?? "";
        }

        private string key = "";
        public IConfiguration Configuration { get; set; } = default!;

        public void ConfigureServices(IServiceCollection services)
        {
            // Adicionar autenticação JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearerDefaults.AuthenticationScheme";
                options.DefaultChallengeScheme = "JwtBearerDefaults.AuthenticationScheme";
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(Configuration["Jwt:Key"] ?? "UmaChaveSecretaMuitoLongaDePeloMenos16Caracteres")
                        ),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization();

            services.AddScoped<iAdministradorServico, AdministradorServico>();
            services.AddScoped<iVeiculosServico, VeiculoServico>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT Aqui"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
                });
            });

            services.AddDbContext<DbContexto>(options =>
            {
                options.UseMySql(
                Configuration.GetConnectionString("MySql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
                );
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                #region HOME COM RECORD STRUCT

                // endpoints.MapGet("/", () => Results.Json(new Home()));
                endpoints.MapGet("/", (HttpRequest request) =>
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
        new Claim("perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil)
        };
                    var token = new JwtSecurityToken(
                        claims: claims,
                        expires: DateTime.Now.AddHours(2),
                        signingCredentials: Credentials
                    );

                    return new JwtSecurityTokenHandler().WriteToken(token);
                }


                endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) =>
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
                endpoints.MapGet("/administradores", ([FromQuery] int? pagina, iAdministradorServico administradorServico) =>
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
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "adm" })
                .WithTags("Administradores");

                // Buscar Veiculo por Id
                endpoints.MapGet("/administradores{id}", ([FromRoute] int id, iAdministradorServico administradorServico) =>
                {
                    var administrador = administradorServico.BuscaPorId(id);

                    if (administrador == null) return Results.NotFound();
                    return Results.Ok(new AdministradorModelView
                    {
                        Id = administrador.Id,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                    });
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "adm" })
                .WithTags("Administradores");

                // Criar Administrador
                endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, iAdministradorServico administradorServico) =>
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

                }).RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "adm" })
                .WithTags("Administradores");
                #endregion

                #region VEICULOS
                // Função de validação do DTO
                ErrosDeValidacao validaDTO(VeiculoDTO VeiculoDTO)
                {
                    var validacao = new ErrosDeValidacao
                    {
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
                endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO VeiculoDTO, iVeiculosServico veiculoServico) =>
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
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "adm,Editor" })
                .WithTags("Veiculos");
                // Buscar Veiculos
                endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, iVeiculosServico veiculoServico) =>
                {
                    var veiculos = veiculoServico.Todos(pagina);
                    return Results.Ok(veiculos);
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "adm,Editor" })
                .WithTags("Veiculos");
                // Buscar Veiculo por Id
                endpoints.MapGet("/veiculos{id}", ([FromRoute] int id, iVeiculosServico veiculoServico) =>
                {
                    var veiculo = veiculoServico.BuscaPorId(id);

                    if (veiculo == null) return Results.NotFound();
                    return Results.Ok(veiculo);
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "adm,Editor" })
                .WithTags("Veiculos");
                // Atualizar Veiculo
                endpoints.MapPut("/veiculos{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, iVeiculosServico veiculoServico) =>
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

                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "adm" })
                .WithTags("Veiculos");
                // Deletar Veiculo
                endpoints.MapDelete("/veiculos{id}", ([FromRoute] int id, iVeiculosServico veiculoServico) =>
                {
                    var veiculo = veiculoServico.BuscaPorId(id);
                    if (veiculo == null) return Results.NotFound();

                    veiculoServico.remover(veiculo);
                    return Results.Ok(veiculo);
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "adm" })
                .WithTags("Veiculos");
                #endregion
            });
        }

    }
