using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Athanasia.Services
{
    public class ServiceLogicApps
    {
        private MediaTypeWithQualityHeaderValue header;
        private string urlLogicApp;

        public ServiceLogicApps(SecretClient secretClient)
        {
            this.header = new MediaTypeWithQualityHeaderValue("application/json");
            KeyVaultSecret urlTriger = secretClient.GetSecret("UrlLogicApp");
            this.urlLogicApp = urlTriger.Value;
        }

        public async Task SendEmailAsync(string email, string asunto, string mensaje)
        {
            var model = new
            {
                email = email,
                asunto = asunto,
                mensaje = mensaje
            };

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(this.urlLogicApp, content);
            }
        }
    }
}
