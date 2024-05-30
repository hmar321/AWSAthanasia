using Athanasia.Models.Tables;
using Athanasia.Models.Util;
using Athanasia.Models.Views;
using Athanasia.Repositories;
using Athanasia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Athanasia.Controllers
{
    public class LibroController : Controller
    {
        private IRepositoryAthanasia repo;
        private IMemoryCache memoryCache;
        private ServiceCacheRedis serviceRedis;

        public LibroController(IRepositoryAthanasia repo, IMemoryCache memoryCache, ServiceCacheRedis serviceRedis)
        {
            this.repo = repo;
            this.memoryCache = memoryCache;
            this.serviceRedis = serviceRedis;
        }

        public async Task<ActionResult> Index(int? posicion, string? busqueda)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            if (busqueda == null)
            {
                busqueda = " ";
            }
            int ndatos = 4;
            PaginacionModel<ProductoSimpleView> model = await this.repo.GetAllProductoSimpleViewSearchPaginacionAsync(busqueda, posicion.Value, ndatos);
            ViewData["POSICION"] = posicion.Value;
            ViewData["PAGINAS"] = model.NumeroPaginas;
            ViewData["BUSQUEDA"] = busqueda;
            return View(model.Lista);
        }
        [HttpPost]
        public async Task<ActionResult> Index(string busqueda)
        {
            int posicion = 1;
            int ndatos = 4;
            PaginacionModel<ProductoSimpleView> model = await this.repo.GetAllProductoSimpleViewSearchPaginacionAsync(busqueda, posicion, ndatos);
            ViewData["POSICION"] = posicion;
            ViewData["PAGINAS"] = model.NumeroPaginas;
            ViewData["BUSQUEDA"] = busqueda;
            return View(model.Lista);
        }

        public async Task<ActionResult> Detalles(int idproducto)
        {
            ProductoView libro = await this.repo.GetProductoByIdAsync(idproducto);
            List<FormatoLibroView> formatos = await this.repo.GetAllFormatoLibroViewByIdLibroAsync(libro.IdLibro);
            ViewData["GENEROS"] = libro.Generos.Split(", ").ToList();
            ViewData["FORMATOS"] = formatos;
            return View(libro);
        }

        public async Task<IActionResult> _IconoCarrito(int idproducto, bool agregar)
        {

            if (HttpContext.User.Identity.IsAuthenticated == false)
            {
                List<ProductoSimpleView> productos = this.memoryCache.Get<List<ProductoSimpleView>>("CARRITO");
                if (agregar == true)
                {
                    if (productos == null)
                    {
                        productos = new List<ProductoSimpleView>();
                    }
                    if (productos.Any(id => id.IdProducto == idproducto) == false)
                    {
                        ProductoSimpleView producto = await this.repo.GetProductoSimpleByIdAsync(idproducto);
                        productos.Add(producto);
                        this.memoryCache.Set("CARRITO", productos);
                    }
                }
            }
            else
            {
                string idusuario = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                List<ProductoSimpleView> productos = await this.serviceRedis.GetProductosFavoritosAsync(idusuario);
                if (agregar == true)
                {
                    ProductoSimpleView producto = await this.repo.GetProductoSimpleByIdAsync(idproducto);
                    await this.serviceRedis.AddProductoFavoritoAsync(idusuario, producto);
                }
            }
            ViewData["IDPRODUCTO"] = idproducto;
            return PartialView("_IconoCarrito");
        }

    }
}
