using Athanasia.Helpers;
using Athanasia.Models.Tables;
using Athanasia.Models.Views;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Athanasia.Services
{
    public class ServiceCacheRedis
    {
        private IDatabase database;

        public ServiceCacheRedis(SecretClient secretClient)
        {
            KeyVaultSecret redisKey = secretClient.GetSecret("CacheRedis");
            database = ConnectionMultiplexer.Connect(redisKey.Value).GetDatabase();
        }

        public async Task AddMultipleFavoritos(string idusuario, List<ProductoSimpleView> productos)
        {
            string jsonProductos = await this.database.StringGetAsync(idusuario);
            List<ProductoSimpleView> productosList;
            if (jsonProductos == null)
            {
                productosList = new List<ProductoSimpleView>();
            }
            else
            {
                productosList = JsonConvert.DeserializeObject<List<ProductoSimpleView>>(jsonProductos);
            }
            foreach (ProductoSimpleView producto in productos)
            {
                ProductoSimpleView prodAlmacenado = productosList.FirstOrDefault(x => x.IdProducto == producto.IdProducto);
                if (prodAlmacenado == null)
                {
                    productosList.Add(producto);
                }
                else
                {
                    int index = productosList.IndexOf(prodAlmacenado);
                    productosList[index] = producto;
                }
            }
            jsonProductos = JsonConvert.SerializeObject(productosList);
            await this.database.StringSetAsync(idusuario, jsonProductos);
        }

        public async Task AddProductoFavoritoAsync(string idusuario, ProductoSimpleView producto)
        {
            string jsonProductos = await this.database.StringGetAsync(idusuario);
            List<ProductoSimpleView> productosList;
            if (jsonProductos == null)
            {
                productosList = new List<ProductoSimpleView>();
            }
            else
            {
                productosList = JsonConvert.DeserializeObject<List<ProductoSimpleView>>(jsonProductos);
            }
            if (productosList.Any(x => x.IdProducto == producto.IdProducto) == false)
            {
                productosList.Add(producto);
                jsonProductos = JsonConvert.SerializeObject(productosList);
                await this.database.StringSetAsync(idusuario, jsonProductos);
            }
        }

        public async Task SaveProductosFavoritoAsync(string idusuario, List<ProductoSimpleView> productos)
        {
            string jsonProductos = JsonConvert.SerializeObject(productos);
            await this.database.StringSetAsync(idusuario, jsonProductos);

        }

        public async Task<List<ProductoSimpleView>> GetProductosFavoritosAsync(string idusuario)
        {
            string jsonProductos = await this.database.StringGetAsync(idusuario);
            if (jsonProductos == null)
            {
                return null;
            }
            else
            {
                List<ProductoSimpleView> favoritos = JsonConvert.DeserializeObject<List<ProductoSimpleView>>(jsonProductos);
                return favoritos;
            }
        }

        public async Task RemoveProductoFavoritoAsync(string idusuario, int idproducto)
        {
            List<ProductoSimpleView> favoritos = await this.GetProductosFavoritosAsync(idusuario);
            if (favoritos != null)
            {
                ProductoSimpleView productoDelete = favoritos.FirstOrDefault(x => x.IdProducto == idproducto);
                favoritos.Remove(productoDelete);
                if (favoritos.Count == 0)
                {
                    await this.database.KeyDeleteAsync(idusuario);
                }
                else
                {
                    string jsonProductos = JsonConvert.SerializeObject(favoritos);
                    await this.database.StringSetAsync(idusuario, jsonProductos, TimeSpan.FromMinutes(30));
                }
            }
        }

        public async Task RemoveCache(string idusuario)
        {
            await this.database.KeyDeleteAsync(idusuario);
        }
    }

}
