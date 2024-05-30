using Athanasia.Models.Tables;
using Athanasia.Models.Util;
using Athanasia.Models.Views;
using Athanasia.Repositories;
using Athanasia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Security.Claims;

namespace Athanasia.Controllers
{
    public class ProductoController : Controller
    {
        private IRepositoryAthanasia repo;
        private IMemoryCache memoryCache;
        private ServiceCacheRedis serviceRedis;

        public ProductoController(IRepositoryAthanasia repo, IMemoryCache memoryCache, ServiceCacheRedis serviceRedis)
        {
            this.repo = repo;
            this.memoryCache = memoryCache;
            this.serviceRedis = serviceRedis;
        }

        public async Task<IActionResult> Carrito()
        {
            return View();
        }

        public async Task<IActionResult> _Precio(int idproducto, int? valor)
        {
            ProductoSimpleView producto;
            if (HttpContext.User.Identity.IsAuthenticated == false)
            {
                List<ProductoSimpleView> productos = memoryCache.Get<List<ProductoSimpleView>>("CARRITO");
                producto = productos.FirstOrDefault(p => p.IdProducto == idproducto);
                if (valor != null)
                {
                    producto.Unidades += valor.Value;
                    if (producto.Unidades < 1)
                    {
                        productos.Remove(producto);
                    }
                    if (productos.Count == 0)
                    {
                        memoryCache.Remove("CARRITO");
                    }
                }
            }
            else
            {
                string idusuario = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                List<ProductoSimpleView> productos = await this.serviceRedis.GetProductosFavoritosAsync(idusuario);
                producto = productos.FirstOrDefault(p => p.IdProducto == idproducto);
                if (valor != null)
                {
                    producto.Unidades += valor.Value;
                    if (producto.Unidades < 1)
                    {
                        await this.serviceRedis.RemoveProductoFavoritoAsync(idusuario, producto.IdProducto);
                    }
                    else
                    {
                        await this.serviceRedis.SaveProductosFavoritoAsync(idusuario, productos);
                    }
                }
            }

            return PartialView("_Precio", producto);
        }

        public async Task<IActionResult> QuitarProductoCarrito(int idproducto)
        {
            if (HttpContext.User.Identity.IsAuthenticated==false)
            {
                List<ProductoSimpleView> productos = this.memoryCache.Get<List<ProductoSimpleView>>("CARRITO");
                ProductoSimpleView producto = productos.FirstOrDefault(prod => prod.IdProducto == idproducto);
                productos.Remove(producto);
                if (productos.Count == 0)
                {
                    this.memoryCache.Remove("CARRITO");
                }
                else
                {
                    this.memoryCache.Set("CARRITO", productos);
                }
            }
            else
            {
                string idusuario = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                await this.serviceRedis.RemoveProductoFavoritoAsync(idusuario,idproducto);
            }
            return RedirectToAction("Carrito");
        }

        public async Task<ActionResult> Buscador(string? busqueda, int? posicion)
        {
            List<Categoria> categs = await this.repo.GetAllCategoriasAsync();
            List<Genero> geners = await this.repo.GetAllGenerosAsync();
            List<int> categorias = memoryCache.Get<List<int>>("FILTROCATEGORIAS");
            List<int> generos = memoryCache.Get<List<int>>("FILTROGENEROS");
            if (categorias == null)
            {
                categorias = categs.Select(c => c.IdCategoria).ToList();
            }
            if (generos == null)
            {
                generos = geners.Select(g => g.IdGenero).ToList();
            }
            if (busqueda == null)
            {
                busqueda = " ";
            }
            if (posicion == null)
            {
                posicion = 1;
            }
            int ndatos = 4;
            PaginacionModel<ProductoSimpleView> model = await this.repo.GetProductoSimpleViewsCategoriasGeneroAsync(busqueda, posicion.Value, ndatos, categorias, generos);
            ViewData["POSICION"] = posicion.Value;
            ViewData["PAGINAS"] = model.NumeroPaginas;
            ViewData["BUSQUEDA"] = busqueda;
            ViewData["CATEGORIAS"] = categs;
            ViewData["GENEROS"] = geners;
            return View(model.Lista);
        }
        [HttpPost]
        public async Task<ActionResult> Buscador(string? busqueda, List<int> categoria, List<int> genero)
        {
            List<Categoria> categs = await this.repo.GetAllCategoriasAsync();
            List<Genero> geners = await this.repo.GetAllGenerosAsync();
            if (categoria.Count == 0)
            {
                categoria = categs.Select(c => c.IdCategoria).ToList();
                memoryCache.Remove("FILTROCATEGORIAS");
            }
            else
            {
                memoryCache.Set("FILTROCATEGORIAS", categoria);
            }
            if (genero.Count == 0)
            {
                genero = geners.Select(g => g.IdGenero).ToList();
                memoryCache.Remove("FILTROGENEROS");
            }
            else
            {
                memoryCache.Set("FILTROGENEROS", genero);
            }
            if (busqueda == null)
            {
                busqueda = " ";
            }
            int posicion = 1;
            int ndatos = 4;
            PaginacionModel<ProductoSimpleView> model = await this.repo.GetProductoSimpleViewsCategoriasGeneroAsync(busqueda, posicion, ndatos, categoria, genero);
            ViewData["POSICION"] = posicion;
            ViewData["PAGINAS"] = model.NumeroPaginas;
            ViewData["BUSQUEDA"] = busqueda;
            ViewData["CATEGORIAS"] = categs;
            ViewData["GENEROS"] = geners;
            return View(model.Lista);
        }

        public async Task<IActionResult> LibrosGenero(string genero)
        {
            Genero gen = await this.repo.GetGeneroByNombreAsync(genero);
            if (gen != null)
            {
                List<int> lista = new List<int> { gen.IdGenero };
                this.memoryCache.Remove("FILTROCATEGORIAS");
                this.memoryCache.Remove("FILTROGENEROS");
                this.memoryCache.Set("FILTROGENEROS", lista);
            }
            return RedirectToAction("Buscador");
        }
    }
}
