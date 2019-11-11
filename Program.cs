using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace HAS.Content
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables();

                    var config = builder.Build();

                    builder.AddAzureKeyVault(
                            $"https://{config["Azure.KeyVault.MPY.Vault"]}.vault.azure.net/",
                            config["Azure.KeyVault.MPY.ClientId"],
                            config["Azure.KeyVault.MPY.ClientSecret"]
                            );

                    builder.AddAzureKeyVault(
                        $"https://{config["Azure.KeyVault.HAS.Vault"]}.vault.azure.net/",
                        config["Azure.KeyVault.HAS.ClientId"],
                        config["Azure.KeyVault.HAS.ClientSecret"]
                        );

                    if (ctx.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets<Startup>();
                    }

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel();
                });
    }
}
