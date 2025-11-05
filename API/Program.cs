using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Projeto_ASP_NET_Minimals_APIs; 

IHostBuilder CreateHostBuilder(string[] args){
  return Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    });
}

CreateHostBuilder(args).Build().Run();

