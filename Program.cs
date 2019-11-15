using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using System;
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

                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    var keyVaultClient = new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(
                            azureServiceTokenProvider.KeyVaultTokenCallback));

                    builder.AddAzureKeyVault(
                        $"https://{config["Azure_KeyVault_MPY_Vault"]}.vault.azure.net/",
                        keyVaultClient,
                        new DefaultKeyVaultSecretManager()
                        );

                    builder.AddAzureKeyVault(
                        $"https://{config["Azure_KeyVault_HAS_Vault"]}.vault.azure.net/",
                        keyVaultClient,
                        new DefaultKeyVaultSecretManager()
                        );

                    if (ctx.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets<Startup>();
                    }

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel((ctx, options) =>
                    {
                        var config = ctx.Configuration;

                        options.Limits.MaxRequestBodySize = 6000000000;
                        options.Limits.MinRequestBodyDataRate =
                            new MinDataRate(bytesPerSecond: 100,
                                gracePeriod: TimeSpan.FromSeconds(10));
                        options.Limits.MinResponseDataRate =
                            new MinDataRate(bytesPerSecond: 100,
                                gracePeriod: TimeSpan.FromSeconds(10));
                        options.Limits.RequestHeadersTimeout =
                            TimeSpan.FromMinutes(2);
                    })
                    .UseStartup<Startup>();
                });
    }
}
