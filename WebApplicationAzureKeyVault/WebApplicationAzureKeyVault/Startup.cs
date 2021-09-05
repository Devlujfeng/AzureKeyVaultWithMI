using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplicationAzureKeyVault
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var msiEnvironment = new MSIEnvironment();
            Configuration.Bind("MSIEnvironment", msiEnvironment);
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", msiEnvironment.TenantId);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", msiEnvironment.ClientId);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", msiEnvironment.ClientSecret);


            string secretName = "isdsDBlogin";
            string keyVaultName = "e1ff54b4592a400291fb546ae1125cf9";
            var kvUri = "https://isdskeyvault.vault.azure.net";
            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                    {
                        Delay= TimeSpan.FromSeconds(2),
                        MaxDelay = TimeSpan.FromSeconds(16),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential
                     }
            };

            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential(), options);

            KeyVaultSecret secret = client.GetSecret(secretName);

            Console.WriteLine("Your secret is '" + secret.Value + "'.");

            System.Threading.Thread.Sleep(5000);
            Console.WriteLine(" done.");
            Console.ReadLine();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    public class MSIEnvironment
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

    }
}
