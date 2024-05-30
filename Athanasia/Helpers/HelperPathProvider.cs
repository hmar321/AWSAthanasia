using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using static NuGet.Packaging.PackagingConstants;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Athanasia.Helpers
{
    public enum FoldersImages { Portadas = 1, Usuarios = 2 }
    public class HelperPathProvider
    {
        private IServer server;
        private IWebHostEnvironment hostEnvironment;
        private string storage;
        public HelperPathProvider(IServer server, IWebHostEnvironment hostEnvironment,SecretClient secretClient)
        {
            this.server = server;
            this.hostEnvironment = hostEnvironment;
            KeyVaultSecret storage = secretClient.GetSecret("UrlStorageBlobs");
            this.storage = storage.Value;
        }
        public string MapUrlServerPath()
        {
            var addresses = server.Features.Get<IServerAddressesFeature>().Addresses;
            string serverUrl = addresses.FirstOrDefault();
            return serverUrl;
        }

        private string GetFolderPath(FoldersImages folder)
        {
            string carpeta = "";
            if (folder == FoldersImages.Portadas)
            {
                carpeta = "images/portadas";
            }
            else if (folder == FoldersImages.Usuarios)
            {
                carpeta = "images/usuarios";
            }
            return carpeta;
        }

        public string MapPath(string fileName, FoldersImages folder)
        {
            string carpeta = this.GetFolderPath(folder);
            string rootPath = this.hostEnvironment.WebRootPath;
            string path = Path.Combine(rootPath, carpeta, fileName);
            return path;
        }

        public string MapUrlPath(string fileName, FoldersImages folder)
        {
            string carpeta = this.GetFolderPath(folder);
            var addresses = server.Features.Get<IServerAddressesFeature>().Addresses;
            string serverUrl = addresses.FirstOrDefault();
            string urlPath = serverUrl + "/" + carpeta + "/" + fileName;
            return urlPath;
        }

        public string GetStorageBlob(string container)
        {
            return this.storage + container + "/";
        }

        public string GetStaticFiles(FoldersImages folder)
        {
            string path = "~/" + this.GetFolderPath(folder);
            return path;
        }
    }
}
