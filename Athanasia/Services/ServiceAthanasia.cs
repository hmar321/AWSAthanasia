using Athanasia.Models;
using Athanasia.Models.Api;
using Athanasia.Models.Tables;
using Athanasia.Models.Util;
using Athanasia.Models.Views;
using Athanasia.Repositories;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Text;

namespace Athanasia.Services
{
    public class ServiceAthanasia : IRepositoryAthanasia
    {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue header;
        private IHttpContextAccessor httpContextAccesor;

        public ServiceAthanasia(SecretClient secretClient, IHttpContextAccessor httpContext)
        {
            KeyVaultSecret urlApiAthanasia = secretClient.GetSecret("ApiAthanasia");
            this.UrlApi = urlApiAthanasia.Value;
            this.header = new MediaTypeWithQualityHeaderValue("application/json");
            this.httpContextAccesor = httpContext;
        }
        #region Metodos
        public string TransformCollectionToQuery(string name, List<string> collection)
        {
            string result = String.Join("&" + name + "=", collection);
            result = "?" + name + "=" + result;
            return result;
        }
        public string TransformCollectionToQuery(string name, List<int> collection)
        {
            string result = String.Join("&" + name + "=", collection);
            result = "?" + name + "=" + result;
            return result;
        }

        public async Task<T> CallGetApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<T> CallGetApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add("authorization", "bearer " + token);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<T> CallPostApiAsync<T, U>(string request, U dataObj)
        {
            using (HttpClient client = new HttpClient())
            {

                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                string json = JsonConvert.SerializeObject(dataObj);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<T> CallPostApiAsync<T, U>(string request, U dataObj, string token)
        {
            using (HttpClient client = new HttpClient())
            {

                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add("authorization", "bearer " + token);
                string json = JsonConvert.SerializeObject(dataObj);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<T> CallPutApiAsync<T, U>(string request, U dataObj)
        {
            using (HttpClient client = new HttpClient())
            {

                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                string json = JsonConvert.SerializeObject(dataObj);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<T> CallPutApiAsync<T, U>(string request, U dataObj, string token)
        {
            using (HttpClient client = new HttpClient())
            {

                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add("authorization", "bearer " + token);
                string json = JsonConvert.SerializeObject(dataObj);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<T> CallDeleteApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {

                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.DeleteAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }
        public async Task<T> CallDeleteApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {

                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add("authorization", "bearer " + token);
                HttpResponseMessage response = await client.DeleteAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        #endregion

        #region Auth
        public async Task<string> AuthTokenAsync(string email, string password)
        {
            string request = "api/auth/login";
            LoginModel dataObj = new LoginModel
            {
                Email = email,
                Password = password
            };
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                string json = JsonConvert.SerializeObject(dataObj);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        #region Categorias
        public async Task<List<Categoria>> GetAllCategoriasAsync()
        {
            string request = "api/categorias";
            return await this.CallGetApiAsync<List<Categoria>>(request);
        }
        #endregion

        #region FormatosLibroView
        public async Task<List<FormatoLibroView>> GetAllFormatoLibroViewByIdLibroAsync(int idlibro)
        {
            string request = "api/FormatosLibroView/FormatosLibro/" + idlibro;
            return await this.CallGetApiAsync<List<FormatoLibroView>>(request);
        }

        #endregion

        #region Generos
        public async Task<List<Genero>> GetAllGenerosAsync()
        {
            string request = "api/Generos/ActivosOrderByNombre";
            return await this.CallGetApiAsync<List<Genero>>(request);
        }

        public async Task<Genero> GetGeneroByNombreAsync(string nombre)
        {
            string request = "api/Generos/GeneroByNombre/" + nombre;
            return await this.CallGetApiAsync<Genero>(request);
        }
        #endregion

        #region InformacionesCompra
        public async Task<InformacionCompra> GetInformacionCompraByIdAsync(int id)
        {
            string request = "api/InformacionesCompra/" + id;
            return await this.CallGetApiAsync<InformacionCompra>(request);
        }

        public async Task<int> DeleteInformacionCompraByIdAsync(int idinfocompra)
        {
            string request = "api/InformacionesCompra/" + idinfocompra;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallDeleteApiAsync<int>(request, token);
        }

        public async Task<List<InformacionCompra>> GetAllInformacionComprabyIdUsuarioAsync(int idusuario)
        {
            string request = "api/InformacionesCompra/ByIdUsuario/" + idusuario;
            return await this.CallGetApiAsync<List<InformacionCompra>>(request);
        }

        public async Task<InformacionCompra> InsertInformacionAsync(string nombre, string direccion, string indicaciones, int idmetodopago, int idusuario)
        {
            InformacionCompra dataObj = new InformacionCompra
            {
                IdInformacionCompra = 0,
                Nombre = nombre,
                Direccion = direccion,
                Indicaciones = indicaciones,
                IdMetodoPago = idmetodopago,
                IdUsuario = idusuario
            };
            string request = "api/InformacionesCompra";
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallPostApiAsync<InformacionCompra, InformacionCompra>(request, dataObj, token);
        }

        #endregion

        #region InformacionCompraView
        public async Task<List<InformacionCompraView>> GetAllInformacionCompraViewByIdUsuarioAsync(int idusuario)
        {
            string request = "api/InformacionesCompraView/ByIdUsuario/" + idusuario;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallGetApiAsync<List<InformacionCompraView>>(request, token);
        }
        #endregion

        #region MetodosPago
        public async Task<List<MetodoPago>> GetMetodoPagosAsync()
        {
            string request = "api/MetodosPago";
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallGetApiAsync<List<MetodoPago>>(request, token);
        }
        #endregion

        #region PedidoProductos
        public async Task<int> InsertListPedidoProductosAsync(int idusuario, List<PedidoProducto> productos)
        {
            string request = "api/PedidoProductos/InsertPedidoProductos/" + idusuario;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallPostApiAsync<int, List<PedidoProducto>>(request, productos, token);
        }
        #endregion

        #region PedidoProductosView
        public async Task<List<PedidoProductoView>> GetPedidoProductoViewsByIdPedidoAsync(int idpedido)
        {
            string request = "api/PedidoProductosView/ByIdPedido/" + idpedido;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallGetApiAsync<List<PedidoProductoView>>(request, token);
        }
        #endregion

        #region Pedidos

        public async Task<int> UpdatePedidoEstadoCancelarAsync(int idpedido)
        {
            string request = "api/Pedidos/CancelarPedido/" + idpedido;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            using (HttpClient client = new HttpClient())
            {

                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add("authorization", "bearer " + token);
                HttpResponseMessage response = await client.PutAsync(request, null);
                if (response.IsSuccessStatusCode)
                {
                    int data = await response.Content.ReadAsAsync<int>();
                    return data;
                }
                else
                {
                    return -1;
                }
            }
        }
        #endregion

        #region PedidosView
        public async Task<List<PedidoView>> GetAllPedidoViewByIdUsuario(int idusuario)
        {
            string request = "api/PedidosView/ByIdUsuario/" + idusuario;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallGetApiAsync<List<PedidoView>>(request, token);
        }
        #endregion

        #region ProductosSimplesView

        public async Task<List<ProductoSimpleView>> GetAllProductoSimpleViewAsync()
        {
            string request = "api/ProductosSimplesView";
            return await this.CallGetApiAsync<List<ProductoSimpleView>>(request);
        }

        public async Task<ProductoSimpleView> GetProductoSimpleByIdAsync(int idproducto)
        {
            string request = "api/ProductosSimplesView/" + idproducto;
            return await this.CallGetApiAsync<ProductoSimpleView>(request);
        }

        public async Task<List<ProductoSimpleView>> GetProductoSimpleViewTituloAutorAsync()
        {
            string request = "api/ProductosSimplesView/DistinctTituloAutor";
            return await this.CallGetApiAsync<List<ProductoSimpleView>>(request);
        }

        public async Task<PaginacionModel<ProductoSimpleView>> GetProductosSimplesPaginacionAsyn(int posicion, int ndatos)
        {
            string request = "api/ProductosSimplesView/Paginacion/" + posicion + "/" + ndatos;
            return await this.CallGetApiAsync<PaginacionModel<ProductoSimpleView>>(request);
        }
        public async Task<PaginacionModel<ProductoSimpleView>> GetAllProductoSimpleViewSearchPaginacionAsync(string palabra, int posicion, int ndatos)
        {
            string request = "api/ProductosSimplesView/PaginacionBusqueda/" + palabra + "/" + posicion + "/" + ndatos;
            return await this.CallGetApiAsync<PaginacionModel<ProductoSimpleView>>(request);
        }

        public async Task<List<ProductoSimpleView>> GetAllProductoSimpleViewByIds(List<int> idsproductos)
        {
            string idsQuery = this.TransformCollectionToQuery("id", idsproductos);
            string request = "api/ProductosSimplesView/ByListId" + idsQuery;
            return await this.CallGetApiAsync<List<ProductoSimpleView>>(request);
        }

        public async Task<PaginacionModel<ProductoSimpleView>> GetProductoSimpleViewsCategoriasGeneroAsync(string palabra, int posicion, int ndatos, List<int> idscategorias, List<int> idsgeneros)
        {
            CategoriasGenerosModel model = new CategoriasGenerosModel
            {
                IdsCategorias = idscategorias,
                IdsGeneros = idsgeneros
            };
            string request = "api/ProductosSimplesView/PaginacionBusquedaCategoriasGeneros/" + palabra + "/" + posicion + "/" + ndatos;
            return await this.CallPostApiAsync<PaginacionModel<ProductoSimpleView>, CategoriasGenerosModel>(request, model);
        }
        #endregion

        #region ProductosView
        public async Task<List<ProductoView>> GetAllProductoViewAsync()
        {
            string request = "api/ProductosView";
            return await this.CallGetApiAsync<List<ProductoView>>(request);
        }

        public async Task<ProductoView> GetProductoByIdAsync(int idproducto)
        {
            string request = "api/ProductosView/" + idproducto;
            return await this.CallGetApiAsync<ProductoView>(request);
        }
        public async Task<List<ProductoView>> GetProductoViewByFormatoAsync(string formato)
        {
            string request = "api/ProductosView/ByFormato/" + formato;
            return await this.CallGetApiAsync<List<ProductoView>>(request);
        }
        #endregion

        #region Usuario
        public async Task<Usuario> FindUsuarioByIdAsync(int idusuario)
        {
            string request = "api/Usuarios/" + idusuario;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallGetApiAsync<Usuario>(request, token);
        }

        public async Task<int> DeleteUsuarioAsync(int idusuario)
        {
            string request = "api/Usuarios/" + idusuario;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallDeleteApiAsync<int>(request, token);
        }

        public async Task<Usuario> AuthGetUsuarioAsync(string token)
        {
            string request = "api/Usuarios/UsuarioToken";
            return await this.CallGetApiAsync<Usuario>(request, token);
        }

        public async Task<Usuario> GetUsuarioByTokenAsync(string tokenmail)
        {
            string request = "api/Usuarios/UsuarioTokenMail/" + tokenmail;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallGetApiAsync<Usuario>(request, token);
        }

        public async Task<Usuario> RegistrarUsuarioAsync(string nombre, string apellido, string email, string password)
        {
            UsuarioPost usuario = new UsuarioPost
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                Password = password
            };
            string request = "api/Usuarios";
            return await this.CallPostApiAsync<Usuario, UsuarioPost>(request, usuario);
        }

        public async Task<Usuario> UpdateUsuarioAsync(int idusuario, string nombre, string apellido, string email, string? imagen)
        {
            UsuarioPut usuario = new UsuarioPut
            {
                IdUsuario = idusuario,
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                Imagen = imagen
            };
            string request = "api/Usuarios";
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            return await this.CallPutApiAsync<Usuario, UsuarioPut>(request, usuario, token);
        }

        public async Task<Usuario> UpdateUsuarioPasswordAsync(int idusuario, string password)
        {
            string request = "api/Usuarios/UpdatePassword/" + idusuario + "/" + password;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add("authorization", "bearer " + token);
                HttpResponseMessage response = await client.PutAsync(request, null);
                if (response.IsSuccessStatusCode)
                {
                    Usuario data = await response.Content.ReadAsAsync<Usuario>();
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<Usuario> UpdateUsuarioTokenAsync(int idusuario)
        {
            string request = "api/Usuarios/UpdateTokenMail/" + idusuario;
            string token = httpContextAccesor.HttpContext.User.FindFirst("TOKENJWT").Value;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add("authorization", "bearer " + token);
                HttpResponseMessage response = await client.PutAsync(request, null);
                if (response.IsSuccessStatusCode)
                {
                    Usuario data = await response.Content.ReadAsAsync<Usuario>();
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }
        public async Task<Usuario> ActivarUsuarioAsync(string tokenmail)
        {
            string request = "api/Usuarios/ActivarUsuario/" + tokenmail;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.PutAsync(request, null);
                if (response.IsSuccessStatusCode)
                {
                    Usuario data = await response.Content.ReadAsAsync<Usuario>();
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }
        public Task<Usuario> LogInUserAsync(string email, string password)
        {
            return null;
        }
        #endregion
    }
}